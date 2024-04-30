namespace Solis.Circuit.Interfaces
{
    /// <summary>
    /// Used to represent a connection between two plugs in a circuit.
    /// </summary>
    public interface ICircuitConnection
    {
        /// <summary>
        /// The first plug in the connection.
        /// </summary>
        public CircuitPlug PlugA { get; set; }
        
        /// <summary>
        /// The second plug in the connection.
        /// </summary>
        public CircuitPlug PlugB { get; set; }
        
        /// <summary>
        /// Checks if the connection is valid (i.e. the plugs are not null).
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Detaches the connection from the plugs.
        /// </summary>
        /// <param name="plug"></param>
        public void Detach(CircuitPlug plug);
    }
}