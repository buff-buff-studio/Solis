using System.IO;
using UnityEngine;

namespace SolarBuff.Data
{
    [System.Serializable]
    public class SaveProfile
    {
        public long modifiedTime;
        public long creationTime;
        public string name;
        public float playTime;
        public Texture2D thumbnail;
        private JsonObject _body;
 
        public JsonObject Body
        {
            get
            {
                if (_body == null)
                    Load();
                
                return _body;
            }
        }

        public void Clear()
        {
            _body = new JsonObject();
        }
        
        public void Load()
        {
            if (string.IsNullOrEmpty(name))
            {
                _body = new JsonObject();
                return;
            }
            
            _body = JsonValue.Parse(File.ReadAllText(SaveManager.SaveFolder + "/" + name + "/body.json")) as JsonObject;
        }
        
        public async Awaitable Save()
        {
            var time = (long) (playTime * System.TimeSpan.TicksPerSecond);
            var modified = System.DateTime.Now.Ticks;

            if (string.IsNullOrEmpty(name))
            {
                name = GenerateSaveName();
            
                while (SaveManager.Instance.Exists(name))
                    name = GenerateSaveName();
            }
            
            if (thumbnail == null)
            {
                await Awaitable.EndOfFrameAsync();
                thumbnail = CreateThumbnail();
            }
            
            var obj = new JsonObject
            {
                ["creationTime"] = creationTime.ToString(),
                ["modifiedTime"] = modified.ToString(),
                ["playTime"] = time.ToString()
            };
            
            var path = SaveManager.SaveFolder + "/" + name;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            await File.WriteAllTextAsync(path + "/data.json", obj.ToString(true));
            await File.WriteAllBytesAsync(path + "/thumbnail.png", thumbnail.EncodeToPNG());
            await File.WriteAllTextAsync(path + "/body.json", Body.ToString(true));
            Debug.Log("Saved to: " + path);
        }
        
        private static Texture2D CreateThumbnail()
        {
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            
            var width = tex.width;
            var height = tex.height;
            const int cropSize = 256;
            
            //TODO: RESCALE IMAGE TO FIT THE SMALLEST SIDE TO 256, THEN CROP IT
            
            var xMin = (width - cropSize) / 2;
            var yMin = (height - cropSize) / 2;
            
            var croppedTexture = new Texture2D(cropSize, cropSize);
            var pixels = tex.GetPixels(xMin, yMin, cropSize, cropSize);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }
        
        private static string GenerateSaveName()
        {
            string[] title = { 
                "Master", "Boss", "Divine", "Different", "Special", "Amazing", "Blaster", "Exceptional", 
                "Incredible", "Unbelievable", "Unreal", "Unrealistic", "Unthinkable"
            };
            
            string[] adjective =
            {
                "Tall", "Short", "Tiny", "Enormous", "Giant", "Red", "Green", "Blue", "Yellow", "Orange", "Brown",
                "Spicy", "Neutral", "Acid", "Basic", "Long", "Troubled", "Simple", "Generic", "Complex", "Complicated",
                "Gorgeous", "Beautiful", "Pretty", "Handsome", "Cute", "Adorable", "Attractive", "Angry", "Mad", "Furious",
            };
            string[] substantive =
            {
                "Cat", "Dog", "Elephant", "Mouse", "Rat", "Snake", "Lion", "Tiger", "Bear", "Wolf", "Fox", "Rabbit",
                "Hare", "Deer", "Horse", "Cow", "Pig", "Sheep", "Goat", "Chicken", "Duck", "Goose", "Turkey", "Pheasant",
                "Hawk", "Eagle", "Falcon", "Owl", "Parrot", "Penguin", "Seagull", "Dove", "Sparrow", "Robin", "Bluebird",
                "Car", "Bus", "Truck", "Van", "Bike", "Motorcycle", "Scooter", "Skateboard", "Rollerblades", "Train", 
                "Subway", "Tram", "Trolley", "Monorail", "Plane", "Helicopter", "Jet", "Rocket", "Spaceship", "UFO",
                "Book", "Pen", "Pencil", "Eraser", "Ruler", "Compass", "Protractor", "Calculator", "Notebook", "Folder",
                "Sofa", "Chair", "Table", "Desk", "Bed", "Cupboard", "Closet", "Drawer", "Shelf", "Bookshelf", "Cabinet",
                "Fridge", "Freezer", "Oven", "Stove", "Microwave", "Dishwasher", "Dryer",
            };
            
            return $"{title[Random.Range(0, title.Length)]} {adjective[Random.Range(0, adjective.Length)]} {substantive[Random.Range(0, substantive.Length)]}";
        }
    }
}