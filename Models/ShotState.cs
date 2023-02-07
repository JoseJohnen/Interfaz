namespace Interfaz.Models
{
    public enum StateOfTheShot { JustCreated = -1, None = 0, Destroyed = 1 }
    public class ShotState
    {
        public int Id { get; set; }
        public StateOfTheShot State { get; set; }

        public ShotState()
        {
            Id = 0;
            State = StateOfTheShot.JustCreated;
        }

        public string ToJson()
        {
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotState) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public ShotState FromJson(string json)
        {
            try
            {
                ShotState shot = System.Text.Json.JsonSerializer.Deserialize<ShotState>(json);
                return shot;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotState) FromJson(): " + ex.Message);
                return new ShotState();
            }
        }

        public static ShotState CreateFromJson(string json)
        {
            try
            {
                ShotState shot = new();
                return shot.FromJson(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (ShotState) CreateFromJson(): " + ex.Message);
                return new ShotState();
            }
        }
    }
}
