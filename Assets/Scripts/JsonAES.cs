using UnityEngine;
using System.IO;
using System.Security.Cryptography;// AES 암호화를 위해 추가
using System.Text;
using System;


public class JsonAES : MonoBehaviour
{
    public PlayerDatas playerData = new PlayerDatas();

    // json 파일 저장 위치
    private string path;
    private string fileName = "/aesSave";

    // AES 암호화를 위한 변수
    /* AES키의 크기에 따른 보안 수준
     * 1. AES-128 (16byte) : 일반적으로 사용되고, 빠름.
     * 2. AES-192 (24byte) : 중간 수준
     * 3. AES-256 (32byte) : 가장 강력하지만, 다소 느림.
     * 
     * * IV는 AES키 길이와 무관하게 항상 16바이트다.
     */
    public byte[] aesKey; // 암호화/복호화에 필요한 키 값
    public byte[] iv; // 초기화 벡터. 암호화 과정에서 입력 데이터의 패턴을 깨뜨려 동일한 텍스트가 동일하게 암호화되지 않도록 한다.
    private string aesKeyPath;
    private string ivPath;


    private void Start()
    {
        path = Application.persistentDataPath + fileName;
        aesKeyPath = Path.Combine(Application.persistentDataPath, "aesKey.dat");
        ivPath = Path.Combine(Application.persistentDataPath, "aesIV.dat");

        Debug.Log(path);

        CheckAESKey();
    }

    // AES키랑 IV가 있는지 확인, 없으면 새로 생성
    private void CheckAESKey()
    {
        if (File.Exists(aesKeyPath) && File.Exists(ivPath)) // 키와 iv가 존대한다면
        {
            aesKey = File.ReadAllBytes(aesKeyPath);
            iv = File.ReadAllBytes(ivPath);
        }
        else // 없다면 새로 생성
        {
            aesKey = GenerateRandomByte(16);
            iv = GenerateRandomByte(16);

            File.WriteAllBytes(aesKeyPath, aesKey);
            File.WriteAllBytes(ivPath, iv);
        }
    }


    // 랜덤 값 생성 함수
    // RNGCryptoServiceProvider클래스는 암호화, 토근 생성, 키 생성 등에서 사용된다.
    private byte[] GenerateRandomByte(int length)
    {
        byte[] randomBytes = new byte[length];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }

        return randomBytes;
    }

    public void SaveData()
    {
        //Debug.Log($"AES KEY (Save) : {Convert.ToBase64String(aesKey)}");
        //Debug.Log($"AES IV (Save) : {Convert.ToBase64String(iv)}");

        string data = JsonUtility.ToJson(playerData);
        File.WriteAllText(path, AESEncrypt(data));

        PrintData();
    }

    public void LoadData()
    {
        if (!File.Exists(path))
            SaveData();

        //Debug.Log($"AES KEY (Load) : {Convert.ToBase64String(aesKey)}");
        //Debug.Log($"AES IV (Load) : {Convert.ToBase64String(iv)}");

        string data = File.ReadAllText(path);
        playerData = JsonUtility.FromJson<PlayerDatas>(AESDecrypt(data));

        PrintData();
    }


    // AES 암호화
    /* < Using을 사용하는 이유! >
     * System.Security.Cryptography 클래스는 내부적으로 "윈도우 API(C/C++ 네이티브 라이브러리)"를 사용한다.
     * .NET이 아닌 운영 체제 수준의 리소스를 사용하기에, .NET의 가비지 컬렉터가 자동으로 관리하지 못한다.
     * 그렇기에 Dispose()를 직접 호출하거나 using 블록으로 감싸야 리소스를 즉시 해제시킬 수 있다.
     * Dispose()를 호출하는 방식보다는 using이 더 안전하고 간결해서 권장된다.
     * 
     * 또한, 내부적으로 네이티브 라이브러리를 사용하는 클래스는 IDisposable 인터페이스를 구현하고 있고,
     * (IDisposable : 수동으로 리소스 해제를 위한 인터페이스)
     * using 키워드는 IDisposable 객체를 자동을 해제한다.
     * 
     * < Using이 안전한 이유 >
     * using은 내부적으로 try-finally로 동작함.
     * 그래서 작업 도중 예외가 발생해도 finally로 Dispose()가 무조건 호출되기 때문에 리소스 누수가 없음.
     */
    private string AESEncrypt(string data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = aesKey;
            aes.IV = iv;

            // 암호화 변환기 생성
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            // 텍스트를 암호화
            byte[] encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);

            // 암호화된 바이트 배열을 Base64 문자열로 변환 후 반환
            return System.Convert.ToBase64String(encrypted);
        }
    }

    // AES 복호화
    private string AESDecrypt(string encryptedText)
    {
        byte[] buffer = System.Convert.FromBase64String(encryptedText);

        using(Aes aes = Aes.Create())
        {
            aes.Key = aesKey;
            aes.IV = iv;

            // 복호화 변환기 생성
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            // 암호화된 바이트 배열을 복호화
            byte[] decrypted = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);

            // 복호화된 바이트 배열을 UTF-8 문자열로 변환 후 반환
            return Encoding.UTF8.GetString(decrypted);
        }
    }

    public void AddData()
    {
        playerData.level += 100;
        playerData.itemLevel[0] += 100;
        playerData.itemLevel[1] += 200;
        playerData.itemLevel[2] += 300;
    }

    private void PrintData()
    {
        Debug.Log($"playerData.level : {playerData.level}");
        Debug.Log($"playerData.itemLevel : {playerData.itemLevel[0]}, {playerData.itemLevel[1]}, {playerData.itemLevel[2]}");
    }
}
