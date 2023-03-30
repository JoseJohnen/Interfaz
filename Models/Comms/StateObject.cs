using System.Text;

namespace Interfaz.Models.Comms
{
    public class StateObject
    {
        // Client socket.  
        //public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        public List<Task> l_tasks = new List<Task>();

        public bool addData(string newData)
        {
            try
            {
                sb.AppendLine(newData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("StateObject.AddData: " + ex.Message);
                return false;
            }
        }
    }
}
