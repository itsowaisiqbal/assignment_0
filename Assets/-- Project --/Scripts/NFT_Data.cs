using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using TMPro;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public class NFT_Data : MonoBehaviour
{
    //i was able to get all this information from the contract number that was available on the OpenSea link provided
    string chain = "polygon";
    string network = "mainnet";
    string contract = "0x80c69CDf4bFC278aBDdF784E6B7f414F771073b2";

    //i have kept the token id as a serializefield so that we can change the token id from the inspector
    [SerializeField]
    string token_id = "0";

    string path_to_file;

    [SerializeField]
    TextMeshProUGUI nft_data;

    [SerializeField]
    GameObject nft_render;

    [SerializeField]
    GameObject progress;

    Slider slider;
    TextMeshProUGUI progress_text;

    float progress_1 = 0;
    float progress_2 = 0;

    public class Response
    {
        //the json file i accessed from the nfts had these fields as their metadata
        //hence i added them as a seperate class to access them later
        public string name;
        public string description;
        public string external_url;
        public string image;
        public string animation_url;
    }

    void Start()
    {
        //progress gameobject is the one which contains the slider and percentage
        slider = progress.transform.Find("Slider").GetComponent<Slider>();
        progress_text = progress.transform.Find("Value").GetComponent<TextMeshProUGUI>();

        //path to file is basically storing the persistentDataPath of the device to save the .mp4 file
        path_to_file = Path.Combine(Application.persistentDataPath, token_id + ".mp4");

        //as soon as the game starts, our main function is called
        NFT_Fetch();
    }

    void Update()
    {
        //the progress values are calculated to give us the downloading progress on our progress bars
        slider.value = (progress_1 + progress_2) / 2;
        progress_text.text = "" + Mathf.Round(slider.value) + "%";
    }

    async void NFT_Fetch()
    {
        //here i am using the erc721 from the chainsafe sdk to access the uri of the nft
        string uri = await ERC721.URI(chain, network, contract, token_id);

        //again using erc721 from the chainsafe sdk but this time to get the owner of the nft
        var ownerOf = await ERC721.OwnerOf(chain, network, contract, token_id);

        var data_request = UnityWebRequest.Get(uri);
        var data_progress = data_request.SendWebRequest();

        //making sure that our data is downloaded before moving forward
        //this is where we will be storing progress_1 as well which is just getting the uri data
        while (!data_progress.isDone)
        {
            await Task.Yield();
            progress_1 = data_request.downloadProgress * 100;
        }

        //if the download somehow fails we will get an error
        if(data_request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("ERROR! - " + data_request.error);
        }


        try
        {
            //once downloaded we format our json data with the Response class we set above 
            Response data = JsonUtility.FromJson<Response>(System.Text.Encoding.UTF8.GetString(data_request.downloadHandler.data));

            //assigning the values to our text field in the canvas
            nft_data.text = "Name: " + data.name + "\n \n" + "Owner: " + ownerOf + "\n \n" + "Description: " + "\n \n" + data.description + "\n \n" + "Visit: " + data.external_url;

            //this part checks if the .mp4 file of the nft is already available in the persistentDataPath
            //this is so that its easier for us to load the .mp4 files the next time the user opens the app rather than downloading them again and again
            if (File.Exists(path_to_file))
            {
                //if it is then it reads it from that location and plays the animation directly
                File.ReadAllBytes(path_to_file);
                Play_Animation(path_to_file);
            }
            else
            {
                //if not found then they will be downloaded
                Download_Animation(data.animation_url);
            }
        }
        //in case something happens an exception will be thrown
        catch(Exception exception)
        {
            Debug.Log("Exception: " + exception.Message);
        }
    }

    async void Download_Animation(string url)
    {
        //using the same technique as before
        var animation_request = UnityWebRequest.Get(url);
        var animation_progress = animation_request.SendWebRequest();

        while (!animation_progress.isDone)
        {
            await Task.Yield();
            progress_2 = animation_request.downloadProgress * 100;
        }

        if(animation_request.result != UnityWebRequest.Result.Success)
        {
            print("ERROR! - " + animation_request.error);
        }

        try
        {
            //if everything is good then it will download and store the video in the persistenDataPath
            var animation_bytes = animation_request.downloadHandler.data; 

            File.WriteAllBytes(path_to_file, animation_bytes);
            print(path_to_file);

            //once downloaded the animation will be played
            Play_Animation(path_to_file);
        }
        //in case something happens an exception will be thrown
        catch (Exception exception)
        {
            Debug.LogError("Exception: " + exception.Message);
        }
    }

    private void Play_Animation(string url)
    {
        //disables the progress bar and enables our nft plane
        progress.SetActive(false);
        nft_render.SetActive(true);

        //video player logic to play the video in our frame
        var video_player = nft_render.GetComponent<VideoPlayer>();
        video_player.url = url;
        
        video_player.Prepare();
        video_player.Play();

    }
}
