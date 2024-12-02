using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string _androidGameId = "YourAndroidGameID";
    [SerializeField] private string _iOSGameId = "YouriOSGameID";
    [SerializeField] private string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] private string _iOSAdUnitId = "Interstitial_iOS";
    [SerializeField] private bool _testMode = true;

    private string _gameId;
    private string _adUnitId;
    private bool _adLoaded = false;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        #if UNITY_IOS
            _gameId = _iOSGameId;
            _adUnitId = _iOSAdUnitId;
        #elif UNITY_ANDROID
            _gameId = _androidGameId;
            _adUnitId = _androidAdUnitId;
        #else
            _gameId = _androidGameId; // For testing in the Editor
            _adUnitId = _androidAdUnitId;
        #endif

        Debug.Log($"Initializing Unity Ads with Game ID: {_gameId}");
        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else
        {
            Debug.Log("Unity Ads is already initialized.");
            OnInitializationComplete(); // Proceed to load the ad if already initialized
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadAd(); // Load an ad immediately after initialization
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
    }

    public void LoadAd()
    {
        Debug.Log($"Loading Ad Unit: {_adUnitId}");
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        if (_adLoaded)
        {
            Debug.Log($"Showing Ad: {_adUnitId}");
            Advertisement.Show(_adUnitId, this);
            _adLoaded = false; // Reset the ad loaded flag
        }
        else
        {
            Debug.LogWarning("Ad not loaded yet. Loading ad now.");
            LoadAd(); // Attempt to load the ad if not loaded
        }
    }

    // Load Listener methods
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Ad Loaded: {adUnitId}");
        _adLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit {adUnitId}: {error} - {message}");
        // Optionally, retry loading the ad after a delay
    }

    // Show Listener methods
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {adUnitId}: {error} - {message}");
        // Optionally, attempt to load another ad
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log($"Ad Show Started: {adUnitId}");
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log($"Ad Clicked: {adUnitId}");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad Show Completed: {adUnitId} - {showCompletionState}");
        // Load the next ad
        LoadAd();
    }
}
