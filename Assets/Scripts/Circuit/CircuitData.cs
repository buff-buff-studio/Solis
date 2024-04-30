namespace Solis.Circuit
{
    /// <summary>
    /// Used to store the power of a circuit and any additional data that may be needed.
    /// </summary>
    public struct CircuitData
    {
        #region Public Fields
        public float power;
        public object additionalData;
        #endregion

        #region Public Constructors
        public CircuitData(float power)
        {
            this.power = power;
            additionalData = null;
        }
        
        public CircuitData(bool power)
        {
            this.power = power ? 1 : 0;
            additionalData = null;
        }
        #endregion
    }
}