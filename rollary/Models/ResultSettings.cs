using System.ComponentModel;

namespace rollary
{
    public class ResultSettings : INotifyPropertyChanged
    {
        private float frameScanWidth = 8;
        public float FrameScanWidth
        {
            get => frameScanWidth;
            set
            {
                frameScanWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrameScanWidth)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ResultSettings Clone() => MemberwiseClone() as ResultSettings;

        public void TransferFrom(ResultSettings source)
        {
            FrameScanWidth = source.FrameScanWidth;
        }

        /// <summary>
        /// Same as `TransferFrom` but ignores some invalid values.
        /// </summary>
        /// <param name="unsafeSource"></param>
        public void SafeTransferFrom(ResultSettings unsafeSource)
        {
            if (!float.IsNaN(unsafeSource.FrameScanWidth) && !float.IsInfinity(unsafeSource.FrameScanWidth) && unsafeSource.FrameScanWidth > 0)
            {
                FrameScanWidth = unsafeSource.FrameScanWidth;
            }
        }
    }
}
