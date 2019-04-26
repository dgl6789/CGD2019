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

        PlayGamesPlatform.DebugLogEnabled = true;

        SignIn();
    }
    
    void SignIn() {
        Social.localUser.Authenticate(
            success => { Debug.Log("Logged In to Google Play Services."); });
    }

    #region Achievements

    public void UnlockAchievement(string id, double progress = 100) {
        Social.ReportProgress(id, progress, success => { Debug.Log("Achievement Unlocked! " + id); });
    }

    public void IncrementAchievement(string id, int stepsToIncrement) {
        PlayGamesPlatform.Instance.IncrementAchievement(id, stepsToIncrement, success => { });
    }

    public void ShowAchievementsUI() {
        Social.ShowAchievementsUI();
    }

    #endregion

    #region Leaderboards

    public void AddScoreToLeaderboard(string leaderboardId, long score) {
        Social.ReportScore(score, leaderboardId, success => { Debug.Log("Score added to leaderboard: " + score); });
    }

    public void ShowLeaderboardUI() {
        Social.ShowLeaderboardUI();
    }

    #endregion
}
