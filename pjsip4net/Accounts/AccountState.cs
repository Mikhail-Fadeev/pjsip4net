using System.Diagnostics;
using pjsip4net.Core.Data;
using pjsip4net.Core.Utils;

namespace pjsip4net.Accounts
{
    /// <summary>
    /// Initial state
    /// </summary>
    internal class InitializingAccountState : AbstractState<RegistrationSession>
    {
        public InitializingAccountState(RegistrationSession owner)
            : base(owner)
        {
            Debug.WriteLine("Account " + _owner.Account.Id + " InitializingAccountState");
            _owner.IsRegistered = false;
        }

        #region Overrides of AccountState

        public override void StateChanged()
        {
            AccountInfo info = _owner.Account.GetAccountInfo();
            if (_owner.Account.IsLocal)
                _owner.ChangeState(new RegisteredAccountState(_owner));
            else if (info.Status == SipStatusCode.Trying)
                _owner.ChangeState(new RegisteringAccountState(_owner));
            else
                _owner.ChangeState(new UnknownStatusState(_owner, info.Status, info.StatusText));
        }

        #endregion
    }

    /// <summary>
    /// After remote registration session started
    /// </summary>
    internal class RegisteringAccountState : AbstractState<RegistrationSession>
    {
        public RegisteringAccountState(RegistrationSession owner)
            : base(owner)
        {
            Debug.WriteLine("Account " + _owner.Account.Id + " RegisteringAccountState");
            _owner.IsRegistered = false;
        }

        #region Overrides of AccountState

        public override void StateChanged()
        {
            AccountInfo info = _owner.Account.GetAccountInfo();
            if (info.Status == SipStatusCode.RequestTimeout)
                _owner.ChangeState(new TimedOutAccountRegistrationState(_owner));
            else if (info.Status == SipStatusCode.Ok)
                _owner.ChangeState(new RegisteredAccountState(_owner));
            else
                _owner.ChangeState(new UnknownStatusState(_owner, info.Status, info.StatusText));
        }

        #endregion
    }

    /// <summary>
    /// Either after PjsipScOk = 200 recieved or local account been added
    /// </summary>
    internal class RegisteredAccountState : AbstractState<RegistrationSession>
    {
        public RegisteredAccountState(RegistrationSession owner)
            : base(owner)
        {
            Debug.WriteLine("Account " + _owner.Account.Id + " RegisteredAccountState");
            _owner.IsRegistered = true;
            //if (_owner.Account.PublishPresence)
            //    _owner.Account.IsOnline = true;
        }

        #region Overrides of AccountState

        public override void StateChanged()
        {
            if (_owner.Account.Id == -1 && _owner.Account.IsLocal)
                _owner.ChangeState(new InitializingAccountState(_owner));
            else
            {
                AccountInfo info = _owner.Account.GetAccountInfo();
                if (info.Status == (SipStatusCode) 1 || info.Status == SipStatusCode.Ok) //OK
                    return;
                //_owner.Account.IsOnline = false;
                if (info.Status == SipStatusCode.RequestTimeout)
                    _owner.ChangeState(new TimedOutAccountRegistrationState(_owner));
                else if (info.Status == SipStatusCode.Trying)
                    _owner.ChangeState(new RegisteringAccountState(_owner));
                else
                    _owner.ChangeState(new UnknownStatusState(_owner, info.Status, info.StatusText));
            }
        }

        #endregion
    }

    /// <summary>
    /// After PjsipScRequestTimeout = 408 recieved 
    /// </summary>
    internal class TimedOutAccountRegistrationState : AbstractState<RegistrationSession>
    {
        public TimedOutAccountRegistrationState(RegistrationSession owner)
            : base(owner)
        {
            Debug.WriteLine("Account " + _owner.Account.Id + " TimedOutAccountRegistrationState");
            _owner.IsRegistered = false;
            //_owner.Account.Dispose();//account can be re-registered - no need to dispose and delete
        }

        #region Overrides of AccountState

        public override void StateChanged()
        {
            AccountInfo info = _owner.Account.GetAccountInfo();
            if (info.Status == SipStatusCode.Ok)
                _owner.ChangeState(new RegisteredAccountState(_owner));
            else if (info.Status == SipStatusCode.Trying)
                _owner.ChangeState(new RegisteringAccountState(_owner));
            else
                _owner.ChangeState(new UnknownStatusState(_owner, info.Status, info.StatusText));
        }

        #endregion
    }

    /// <summary>
    /// After unknown status recieved
    /// </summary>
    internal class UnknownStatusState : AbstractState<RegistrationSession>
    {
        public UnknownStatusState(RegistrationSession owner, SipStatusCode code, string statusText)
            : base(owner)
        {
            _owner.IsRegistered = false;
            StatusCode = code;
            StatusText = statusText;
            Debug.WriteLine("Account " + _owner.Account.Id + " UnknownStatusState");
            Debug.WriteLine(StatusText);
        }

        public SipStatusCode StatusCode { get; private set; }
        public string StatusText { get; private set; }

        public override void StateChanged()
        {
            AccountInfo info = _owner.Account.GetAccountInfo();
            if (info.Status == SipStatusCode.Ok)
                _owner.ChangeState(new RegisteredAccountState(_owner));
            else if (info.Status == SipStatusCode.RequestTimeout)
                _owner.ChangeState(new TimedOutAccountRegistrationState(_owner));
            else if (info.Status == SipStatusCode.Trying)
                _owner.ChangeState(new RegisteringAccountState(_owner));
            else
                _owner.ChangeState(new UnknownStatusState(_owner, info.Status, info.StatusText));
        }
    }
}