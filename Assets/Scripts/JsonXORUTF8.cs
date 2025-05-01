using UnityEngine;
using System.IO;


/* JSON을 암호화 하는 방법
 * 1. XOR -> 단순한 방법의 암호화
 * 2. UTF8 -> 이건 암호화가 아니라 단순 인코딩임.
 * 3. AES -> 셋 중에선 이게 제일 강력한 암호화 방법인듯.
 */


// 직렬화를 함으로써 저장하거나 네트워크 전송이 가능해진다.
[System.Serializable]
public class PlayerDatas
{
    public int level = 0;
    public int[] itemLevel = { 0, 0, 0 };
}


public class JsonXORUTF8 : MonoBehaviour
{
    public PlayerDatas playerData = new PlayerDatas();

    // json 파일 저장 위치
    private string path;
    private string fileName = "/save";

    // XOR 암호화를 위한 키
    private string xorKey = "alwth 1rr!3@(*$ ^2";

    void Start()
    { 
        // Application.dataPath는 json파일이 Asset 폴더에 생긴다(에디터에서만 된다)
        path = Application.persistentDataPath + fileName;

        Debug.Log(path);

        LoadData();
    }

    public void SaveData()
    {
        string data = JsonUtility.ToJson(playerData);

        // 1. XOR로 암호화
        File.WriteAllText(path, XOREncryptAndDecrypt(data));
        // 2. UTF8로 암호화
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        string code = System.Convert.ToBase64String(bytes);
        File.WriteAllText(path, code);


        PrintData();
    }

    public void LoadData()
    {
        if (!File.Exists(path))
            SaveData();

        string data = File.ReadAllText(path);

        // 1. XOR로 복호화
        playerData = JsonUtility.FromJson<PlayerDatas>(XOREncryptAndDecrypt(data));
        // 2. UTF8로 복호화
        byte[] bytes = System.Convert.FromBase64String(data);
        string jdata = System.Text.Encoding.UTF8.GetString(bytes);
        playerData = JsonUtility.FromJson<PlayerDatas>(jdata);


        PrintData();
    }

    // XOR 암호화 & 복호화
    private string XOREncryptAndDecrypt(string data)
    {
        string result = "";

        for (int i = 0; i < data.Length; i++)
        {
            // XOR 비트 연산
            // 같으면 0, 다르면 1
            result += (char)(data[i] ^ xorKey[i % xorKey.Length]);
        }

        return result;
    }

    public void AddData()
    {
        playerData.level += 1;
        playerData.itemLevel[0] += 1;
        playerData.itemLevel[1] += 2;
        playerData.itemLevel[2] += 3;
    }

    private void PrintData()
    {
        Debug.Log($"playerData.level : {playerData.level}");
        Debug.Log($"playerData.itemLevel : {playerData.itemLevel[0]}, {playerData.itemLevel[1]}, {playerData.itemLevel[2]}");
    }
}
