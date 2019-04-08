using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

/// <summary>
/// Manages Google Play Games features and services. Includes loading and distribution of data.
/// </summary>
public class GPManager : MonoBehaviour
{
    public static GPManager Instance;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(this);

        if (this) Load();
    }

    // Start is called before the first frame update
    void Load()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);

        PlayGamesPlatform.Activate();
    }
    
    void SignIn() {
        Social.localUser.Authenticate(success => { });
    }

    #region Achievements

    public void UnlockAchievement(string id, double progress = 100) {
        Social.ReportProgress(id, progress, success => { });
    }

    public void IncrementAchievement(string id, int stepsToIncrement) {
        PlayGamesPlatform.Instance.IncrementAchievement(id, stepsToIncrement, success => { });
    }

    public void ShowAchievementsUI() {
        Debug.Log("Showing Achievements UI");
        Social.ShowAchievementsUI();
    }

    #endregion

    #region Leaderboards

    public void AddScoreToLeaderboard(string leaderboardId, long score) {
        Social.ReportScore(score, leaderboardId, success => { });
    }

    public void ShowLeaderboardUI() {
        Debug.Log("Showing Leaderboard UI");
        Social.ShowLeaderboardUI();
    }

    #endregion
}
