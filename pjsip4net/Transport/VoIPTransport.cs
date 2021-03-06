using pjsip4net.Core;
using pjsip4net.Core.Data;
using pjsip4net.Core.Interfaces;
using pjsip4net.Core.Interfaces.ApiProviders;
using pjsip4net.Core.Utils;
using pjsip4net.Interfaces;

namespace pjsip4net.Transport
{
    internal abstract class VoIPTransport : Initializable, IVoIPTransport
    {
        #region Protected Data

        internal TransportConfig _config;
        internal TransportInfo _info;
        internal TransportType _transportType;

        protected ITransportApiProvider _transportApiProvider;

        #endregion

        #region Properties

        public TransportType TransportType
        {
            get { return _transportType; }
        }

        //public uint Port
        //{
        //    get
        //    {
        //        GuardDisposed();
        //        return _config.Port;
        //    }
        //    set
        //    {
        //        GuardDisposed();
        //        GuardNotInitializing();
        //        _config.Port = value;
        //    }
        //}

        //public string PublicAddress
        //{
        //    get
        //    {
        //        GuardDisposed();
        //        return _config.PublicAddress;
        //    }
        //    set
        //    {
        //        GuardDisposed();
        //        GuardNotInitializing();
        //        _config.PublicAddress = value;
        //    }
        //}

        //public string BoundAddress
        //{
        //    get
        //    {
        //        GuardDisposed();
        //        return _config.BoundAddress;
        //    }
        //    set
        //    {
        //        GuardDisposed();
        //        GuardNotInitializing();
        //        _config.BoundAddress = value;
        //    }
        //}

        public string TransportName
        {
            get
            {
                GuardDisposed();
                if (!Equals(_info, default(TransportInfo)))
                    return _info.TypeName;
                return "";
            }
        }

        public string TransportDescription
        {
            get
            {
                GuardDisposed();
                if (!Equals(_info, default(TransportInfo)))
                    return _info.Info;
                return "";
            }
        }

        public bool? IsReliable
        {
            get
            {
                GuardDisposed();
                if (!Equals(_info, default(TransportInfo)))
                    return ((TransportFlags) _info.Flag & TransportFlags.Reliable) != 0;
                return null;
            }
        }

        public bool? IsSecure
        {
            get
            {
                GuardDisposed();
                if (!Equals(_info, default(TransportInfo)))
                    return ((TransportFlags) _info.Flag & TransportFlags.Secure) != 0;
                return null;
            }
        }

        public TransportConfig Config
        {
            get
            {
                GuardDisposed();
                return _config;
            }
        }

        public void SetConfig(TransportConfig config)
        {
            GuardNotInitializing();
            Helper.GuardNotNull(config);
            _config = config;
        }

        public void SetId(int id)
        {
            Id = id;
        }

        public int Id { get; internal set; }

        //public string LocalName
        //{
        //    get
        //    {
        //        GuardDisposed();
        //        if (!Equals(_info, default(pjsua_transport_info)))
        //            return _info.local_name.host + ":" + _info.local_name.port;
        //        return "";
        //    }
        //}

        #endregion

        protected VoIPTransport(ITransportApiProvider transportApiProvider) 
        {
            Helper.GuardNotNull(transportApiProvider);
            _transportApiProvider = transportApiProvider;
            Id = -1;
        }

        #region Interfaces implementations

        protected override void CleanUp()
        {
            //if (Id != -1)
            //    _transportApiProvider.CloseTransport(Id);
        }

        public override void BeginInit()
        {
            base.BeginInit();
            _config = _transportApiProvider.GetDefaultConfig();
            _config.Port = 5060;
        }

        public override void EndInit()
        {
            base.EndInit();
            Helper.GuardInRange(1u, 65535u, Config.Port);
        }

        #endregion

        #region Implementation of IEquatable<IIdentifiable<IVoIPTransport>>

        public bool Equals(IIdentifiable<IVoIPTransport> other)
        {
            return EqualsTemplate.Equals(this, other);
        }

        public virtual bool DataEquals(IVoIPTransport other)
        {
            return Config.Port.Equals(other.Config.Port) && TransportType.Equals(other.TransportType);
        }

        #endregion
    }
}