using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ScreenReceiver : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 9999;

    private TcpClient client;
    private NetworkStream networkStream;
    private Thread receiveThread;
    private RawImage rawImage;

    void Start()
    {
        // Set up the RawImage component
        rawImage = GetComponent<RawImage>();

        // Connect to the server
        client = new TcpClient(serverIP, serverPort);
        networkStream = client.GetStream();

        // Start the receive thread
        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
    }

    //void ReceiveData()
    //{
    //    byte[] lengthBuffer = new byte[4];
    //    while (true)
    //    {
    //        try
    //        {
    //            // Receive the image length
    //            int bytesRead = 0;
    //            while (bytesRead < lengthBuffer.Length)
    //            {
    //                int read = networkStream.Read(lengthBuffer, bytesRead, lengthBuffer.Length - bytesRead);
    //                if (read == 0) throw new Exception("Connection closed");
    //                bytesRead += read;
    //            }

    //            // int length = BitConverter.ToInt32(lengthBuffer, 0);
    //            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

    //            if (length <= 0 || length > 100000000) // Adjust this value as per your requirement
    //                throw new Exception("Invalid image length");

    //            // Receive the image data
    //            byte[] imageData = new byte[length];
    //            bytesRead = 0;
    //            while (bytesRead < imageData.Length)
    //            {
    //                int read = networkStream.Read(imageData, bytesRead, imageData.Length - bytesRead);
    //                if (read == 0) throw new Exception("Connection closed");
    //                bytesRead += read;
    //            }

    //            // Run the texture update code on the main thread
    //            if (UnityMainThreadDispatcher.Instance() != null)
    //            {
    //                UnityMainThreadDispatcher.Instance().Enqueue(() => {
    //                    // Load the image into a Texture2D
    //                    Texture2D texture = new Texture2D(2, 2);
    //                    texture.LoadImage(imageData);

    //                    // Update the RawImage component
    //                    rawImage.texture = texture;
    //                    rawImage.SetNativeSize();
    //                });
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogError(e);
    //            break;
    //        }
    //    }
    //}

    void ReceiveData()
    {
        byte[] lengthBuffer = new byte[4];
        while (true)
        {
            try
            {
                // Receive the image length
                int bytesRead = 0;
                while (bytesRead < lengthBuffer.Length)
                {
                    int read = networkStream.Read(lengthBuffer, bytesRead, lengthBuffer.Length - bytesRead);
                    if (read == 0) throw new Exception("Connection closed");
                    bytesRead += read;
                }

                // int length = BitConverter.ToInt32(lengthBuffer, 0);
                int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));
                if (length <= 0 || length > 100000000) // Adjust this value as per your requirement
                    throw new Exception("Invalid image length");

                // Receive the image data
                byte[] imageData = new byte[length];
                bytesRead = 0;
                while (bytesRead < imageData.Length)
                {
                    int read = networkStream.Read(imageData, bytesRead, imageData.Length - bytesRead);
                    if (read == 0) throw new Exception("Connection closed");
                    bytesRead += read;
                }
                Debug.Log($"Received image data of size {imageData.Length} bytes");
                // Run the texture update code on the main thread

                Debug.Log($"UnityMainThreadDispatcher instance is null: {UnityMainThreadDispatcher.Instance() == null}");

                if (UnityMainThreadDispatcher.Instance() != null)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        Debug.Log("Entering texture update code");
                        if (rawImage == null) return;
                        Debug.Log("CODE TWO");
                        // Load the image into a Texture2D
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(imageData);
                        Debug.Log($"Applying texture of size {texture.width}x{texture.height}");

                        // Update the RawImage component
                        rawImage.texture = texture;
                        rawImage.SetNativeSize();
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                break;
            }
        }
    }

    void OnDestroy()
    {
        // Clean up
        if (receiveThread != null)
            receiveThread.Abort();
        if (networkStream != null)
            networkStream.Close();
        if (client != null)
            client.Close();
    }
}