using CardinalEngine;
using System.Numerics;

namespace Example {

    public class TestGame {
        private Cardinal cardinal = new Cardinal();
        private Space space;

        public TestGame() {
            space = cardinal.AddSpace();
            cardinal.OnPlayerConnected += OnPlayerConnected;
            cardinal.Start();
            Console.ReadLine();
        }

        private void OnPlayerConnected(NetPlayer netPlayerHandler) {
            Random rnd = new Random();
            int num = rnd.Next() % 10;
            NetEntity entity = new NetEntity(space, new Vector3(num - 5, 0, 0));
            entity.AddComponent<Observer>().Link(netPlayerHandler, true);
            entity.AddComponent<TestComponent>();
        }
    }
}