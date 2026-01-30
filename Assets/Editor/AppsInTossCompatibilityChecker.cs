#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AppsInTossCompatibilityChecker : EditorWindow
{
    [MenuItem("AppsInToss/Compatibility Checker")]
    public static void ShowWindow()
    {
        GetWindow<AppsInTossCompatibilityChecker>("AppsInToss 호환성 검사");
    }
    
    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("AppsInToss 프로젝트 호환성 및 최적화 분석", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("토스 미니앱은 저사양 환경과 빠른 로딩이 중요합니다. 이 도구는 프로젝트의 리소스를 분석하여 가이드를 제안합니다.", MessageType.Info);
        
        GUILayout.Space(10);
        if (GUILayout.Button("분석 시작", GUILayout.Height(40)))
        {
            AnalyzeProject();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("모든 텍스처 1024px로 일괄 최적화", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("텍스처 최적화", "프로젝트의 모든 텍스처 최대 크기를 1024로 제한하시겠습니까? (WebGL 빌드용)", "예", "아니오"))
            {
                OptimizeAllTextures(1024);
            }
        }
    }

    void OptimizeAllTextures(int maxSize)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("WebGL");
                if (!settings.overridden) 
                {
                    settings.overridden = true;
                    settings.name = "WebGL";
                }
                
                if (settings.maxTextureSize > maxSize)
                {
                    settings.maxTextureSize = maxSize;
                    settings.format = TextureImporterFormat.Automatic;
                    importer.SetPlatformTextureSettings(settings);
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }
        Debug.Log($"<b>[최적화 완료]</b> {count}개의 텍스처가 {maxSize}px로 최적화되었습니다.");
        AnalyzeProject(); 
    }
    
    void AnalyzeProject()
    {
        Debug.ClearDeveloperConsole();
        Debug.Log("<b>[AppsInToss 분석 시작]</b>");

        var textures = GetAllTexturesSortedByMemory();
        float textureMemory = textures.Sum(t => t.memory);
        float meshMemory = CalculateMeshMemoryUsage();
        
        Debug.Log($"<color=cyan>예상 총 텍스처 메모리: {textureMemory:F2}MB</color>");
        Debug.Log($"<color=cyan>예상 총 메시 메모리: {meshMemory:F2}MB</color>");
        
        Debug.Log("--- <b>용량 상위 10개 텍스처 (지우거나 줄여야 할 후보)</b> ---");
        int limit = Mathf.Min(10, textures.Count);
        for (int i = 0; i < limit; i++)
        {
            Debug.Log($"[{i+1}] {textures[i].name}: <color=yellow>{textures[i].memory:F2}MB</color> ({textures[i].width}x{textures[i].height})\n<color=grey>경로: {textures[i].path}</color>");
        }

        CheckIncompatibleComponents();
        SuggestOptimizations(textureMemory, meshMemory);
        
        Debug.Log("<b>[분석 완료] 콘솔창의 로그를 확인해주세요.</b>");
    }

    struct TextureInfo { public string name; public float memory; public int width; public int height; public string path; }

    List<TextureInfo> GetAllTexturesSortedByMemory()
    {
        List<TextureInfo> list = new List<TextureInfo>();
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                float mem = (tex.width * tex.height * 4) / (1024f * 1024f);
                list.Add(new TextureInfo { name = tex.name, memory = mem, width = tex.width, height = tex.height, path = path });
            }
        }
        return list.OrderByDescending(t => t.memory).ToList();
    }

    float CalculateTextureMemoryUsage() 
    {
        return GetAllTexturesSortedByMemory().Sum(t => t.memory);
    }

    float CalculateMeshMemoryUsage()
    {
        float totalMemory = 0;
        string[] guids = AssetDatabase.FindAssets("t:Mesh");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null)
            {
                totalMemory += (mesh.vertexCount * 40f) / (1024f * 1024f);
            }
        }
        return totalMemory;
    }

    void CheckIncompatibleComponents()
    {
        Debug.Log("--- 호환성 체크 ---");
        
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            Debug.LogWarning("⚠️ 현재 빌드 타겟이 WebGL이 아닙니다. 토스 미니앱은 WebGL 기반입니다.");
        }

        string[] shaderGuids = AssetDatabase.FindAssets("t:Shader");
        if (shaderGuids.Length > 20)
        {
            Debug.LogWarning($"⚠️ 프로젝트에 셰이더가 너무 많습니다({shaderGuids.Length}개). WebGL 빌드 시간과 런타임 오버헤드가 증가할 수 있습니다.");
        }
    }

    void SuggestOptimizations(float texMem, float meshMem)
    {
        Debug.Log("--- 권장 최적화 제안 ---");

        if (texMem > 50f)
        {
            Debug.LogWarning("💡 텍스처 메모리 사용량이 높습니다(50MB 초과). 모든 텍스처의 'Max Size'를 512나 1024로 제한하는 것을 권장합니다.");
        }

        if (meshMem > 10f)
        {
            Debug.LogWarning("💡 메시 메모리 사용량이 높습니다. 폴리곤 수가 너무 많은 모델이 있는지 확인하고 Mesh Compression을 활성화하세요.");
        }

        Debug.Log("💡 WebGL 배포 시 'Code Stripping' 수준을 'High'로 설정하여 파일 크기를 최적화하세요.");
    }
}
#endif