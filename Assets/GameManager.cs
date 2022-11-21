using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public GameObject menuRoot;
    public GameObject gameRoot;
    public GameObject[] parts;
    public GameObject instructions;
    public TextMeshPro pointsText;
    public GameObject[] history;
    private GameStates _gameState = GameStates.Menu;

    private enum GameStates
    {
        Menu,
        Game,
        Report
    }

    private void Start()
    {
        menuRoot.SetActive(true);
        instructions.SetActive(true);
        gameRoot.SetActive(false);
        foreach (var go in history)
        {
            go.SetActive(false);
        }

        instructions.GetComponent<TextMeshPro>().text = "Press the space bar to stop the map at the correct position";

        foreach (var part in parts)
        {
            var localPosition = part.transform.localPosition;
            localPosition = new Vector3(
                -14,
                localPosition.y,
                localPosition.z
            );
            part.transform.localPosition = localPosition;
        }
    }

    private void ToGame()
    {
        _gameState = GameStates.Game;
        pointsText.text = "";
        menuRoot.SetActive(false);
        gameRoot.SetActive(true);
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var part in parts)
        {
            var speed = new Random().Next(6, 12);
            while (part.transform.localPosition.x < 7)
            {
                part.transform.Translate(Vector3.right * Time.deltaTime * speed);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    break;
                }

                yield return null;
            }

            yield return null;
        }

        instructions.SetActive(false);

        var points = Mathf.RoundToInt(Mathf.Max(0, parts.Sum(x => 100 - Mathf.Abs(x.transform.localPosition.x) * 200)));
        var startPoint = 0;
        while (startPoint < points)
        {
            startPoint += 5;
            startPoint = Math.Min(points, startPoint);
            pointsText.text = "Points: " + startPoint;
            yield return null;
        }

        foreach (var part in parts)
        {
            StartCoroutine(ToZeroPosition(part));
        }

        yield return new WaitForSeconds(1.0f);

        foreach (var go in history)
        {
            go.SetActive(true);
            var text = go.GetComponentInChildren<TextMeshPro>();
            var sprite = go.GetComponentInChildren<SpriteRenderer>();
            text.color = new Color(1, 1, 1, 0);
            sprite.color = new Color(1, 1, 1, 0);
            const int startAlpha = 0;
            const int endAlpha = 1;
            var start = Time.timeSinceLevelLoad;
            while (Time.timeSinceLevelLoad - start < 0.5)
            {
                var alpha = Mathf.Lerp(startAlpha, endAlpha, Time.timeSinceLevelLoad - start);
                text.color = new Color(1, 1, 1, alpha);
                sprite.color = new Color(1, 0, 0, alpha);
                yield return null;
            }

            text.color = new Color(1, 1, 1, 1);
            sprite.color = new Color(1, 0, 0, 1);

            yield return new WaitForSeconds(2.5f);
        }

        instructions.GetComponent<TextMeshPro>().text = "Press Space to Restart";
        instructions.SetActive(true);

        _gameState = GameStates.Report;
    }

    private static IEnumerator ToZeroPosition(GameObject go)
    {
        var start = Time.timeSinceLevelLoad;
        var startX = go.transform.localPosition.x;
        while (Time.timeSinceLevelLoad - start < 1)
        {
            var x = Mathf.Lerp(startX, 0, Time.timeSinceLevelLoad - start);
            var localPosition = go.transform.localPosition;
            localPosition = new Vector3(x, localPosition.y, localPosition.z);
            go.transform.localPosition = localPosition;
            yield return null;
        }

        var pos = go.transform.localPosition;
        pos = new Vector3(0, pos.y, pos.z);
        go.transform.localPosition = pos;
    }

    private void Update()
    {
        if (_gameState == GameStates.Game)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        switch (_gameState)
        {
            case GameStates.Menu:
                ToGame();
                break;
            case GameStates.Report:
                Start();
                ToGame();
                break;
            case GameStates.Game:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}