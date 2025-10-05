using System.Collections;
using Unity.Cinemachine;
using UnityEngine;


public enum StateOnBattle
{
    Start,
    BlueTeamTurn,
    RedTeamTurn,
    AnimationPlay,
    Finish
}   
public class BattleHandler : MonoBehaviour
{
    [SerializeField] private StateOnBattle currentState;

    [Header("Setup Team")]
    private TeamID TeamID;
    public TeamID teamID { get { return TeamID; } set { TeamID = value; } }

    [Header("Set up Canvas")]
    [SerializeField] private GameObject _battleCanvas;
    [SerializeField] private GameObject _blueTeamPanel;
    [SerializeField] private GameObject _redTeamPanel;
    [SerializeField] private GameObject _timerInTurn;
    [SerializeField] private GameObject _randomWindInTurn;
    [SerializeField] private GameObject _skillMenu;
    [SerializeField] private GameObject _itemMenu;
    [SerializeField] private GameObject _characterInfoPanel;

    [SerializeField] private GameObject _finishCanvas;
    [SerializeField] private GameObject _TeamWinPanel;
    [SerializeField] private GameObject _TeamLosePanel;


    [Header("Set up Cameras")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CinemachineCamera _startBattle ;
    [SerializeField] private CinemachineCamera _blueTeamCamera;
    [SerializeField] private CinemachineCamera _redTeamCamera;
    [SerializeField] private CinemachineCamera _animationCamera;

    [Header("Set up Audio")]
    [SerializeField] private AudioSource _battleMusic;
    [SerializeField] private AudioSource _victoryMusic;
    [SerializeField] private AudioSource _defeatMusic;
    [SerializeField] private AudioSource _turnSound;
    [SerializeField] private AudioSource _selectSound;
    [SerializeField] private AudioSource _attackSound;
    [SerializeField] private AudioSource _skillSound;
    [SerializeField] private AudioSource _itemSound;

    [Header("Set up Time Game")]
    [SerializeField] private float _timePerTurn = 20f;




    public StateOnBattle CurrentState { get { return currentState; } set { currentState = value; } }
    private void Awake()
    {
        currentState = StateOnBattle.Start;
        _battleMusic = GetComponent<AudioSource>();
        _battleMusic.Play();
    }

    private void Update()
    {
        switch (currentState)
        {
            case StateOnBattle.Start:
                // Khởi đầu trận đấu
                Debug.Log("Battle Start!");
                // Chuyển sang lượt của đội xanh
                currentState = StateOnBattle.BlueTeamTurn;
                break;
            case StateOnBattle.BlueTeamTurn:
                // Xử lý lượt của đội xanh
                Debug.Log("Blue Team's Turn");
                // Sau khi hoàn thành lượt, chuyển sang trạng thái AnimationPlay
                currentState = StateOnBattle.AnimationPlay;
                break;
            case StateOnBattle.RedTeamTurn:
                // Xử lý lượt của đội đỏ
                Debug.Log("Red Team's Turn");
                // Sau khi hoàn thành lượt, chuyển sang trạng thái AnimationPlay
                currentState = StateOnBattle.AnimationPlay;
                break;
            case StateOnBattle.AnimationPlay:
                // Chơi hoạt ảnh tấn công hoặc kỹ năng
                Debug.Log("Playing Animation...");
                // Sau khi hoàn thành hoạt ảnh, chuyển sang lượt của đội tiếp theo
                if (currentState == StateOnBattle.BlueTeamTurn)
                {
                    currentState = StateOnBattle.RedTeamTurn;
                }
                else
                {
                    currentState = StateOnBattle.BlueTeamTurn;
                }
                break;
            case StateOnBattle.Finish:
                // Kết thúc trận đấu
                Debug.Log("Battle Finished!");
                break;
        }
    }

    public void EndBattle()
    {
        currentState = StateOnBattle.Finish;
    }

    public void BlueTeamTurn()
    {
        currentState = StateOnBattle.BlueTeamTurn;

        
    }

    private void RedTeamTurn()
    {
        currentState = StateOnBattle.RedTeamTurn;
    }

    IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private void ChangeStateAfterAnimation()
    {

        StartCoroutine(WaitForSeconds(2f));
    }
}
