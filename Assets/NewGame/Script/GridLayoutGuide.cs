using UnityEngine;
using UnityEngine.UI;

public class GridLayoutGuide : MonoBehaviour
{
    [Header("📋 Grid Layout Group 설정 가이드")]
    [TextArea(8, 12)]
    public string gridGuide = 
        "🎯 Grid Layout Group 설정 방법:\n\n" +
        "1️⃣ UI 계층 구조:\n" +
        "Canvas → InventoryPanel → SlotParent\n\n" +
        "2️⃣ SlotParent에 Grid Layout Group 추가:\n" +
        "Inspector → Add Component → Grid Layout Group\n\n" +
        "3️⃣ 추천 설정값:\n" +
        "• Cell Size: 70x70 (슬롯 크기)\n" +
        "• Spacing: 10x10 (간격)\n" +
        "• Constraint: Fixed Column Count\n" +
        "• Constraint Count: 5 (한 줄에 5개)\n\n" +
        "4️⃣ 자동 설정: 아래 버튼 클릭!";

    [Header("🚀 자동 설정")]
    [Tooltip("현재 오브젝트에 Grid Layout Group을 자동으로 설정합니다")]
    public bool autoSetupGridLayout = false;
    
    [Header("⚙️ Grid Layout 설정값")]
    [Tooltip("각 슬롯의 크기")]
    public Vector2 cellSize = new Vector2(70, 70);
    
    [Tooltip("슬롯 간 간격")]
    public Vector2 spacing = new Vector2(10, 10);
    
    [Tooltip("한 줄에 표시할 슬롯 개수")]
    [Range(3, 10)]
    public int columnsPerRow = 5;
    
    [Tooltip("시작 모서리")]
    public GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft;
    
    [Tooltip("배치 방향 (가로 우선 / 세로 우선)")]
    public GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Horizontal;
    
    [Tooltip("자식 요소들의 정렬")]
    public TextAnchor childAlignment = TextAnchor.UpperLeft;

    private GridLayoutGroup gridLayoutGroup;

    void Start()
    {
        if (autoSetupGridLayout)
        {
            SetupGridLayout();
            autoSetupGridLayout = false;
        }
    }

    void Update()
    {
        // G키를 누르면 Grid Layout 설정
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetupGridLayout();
        }
    }

    [ContextMenu("Setup Grid Layout")]
    public void SetupGridLayout()
    {
        // 기존 Grid Layout Group 찾기 또는 추가
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
            // Debug.Log("✅ Grid Layout Group 컴포넌트가 추가되었습니다!");
        }

        // 설정 적용
        ApplyGridSettings();
        
        // Debug.Log($"🎯 Grid Layout Group 설정 완료!\n" +
        //           $"Cell Size: {cellSize}\n" +
        //           $"Spacing: {spacing}\n" +
        //           $"Columns: {columnsPerRow}");
    }

    void ApplyGridSettings()
    {
        if (gridLayoutGroup == null) return;

        gridLayoutGroup.cellSize = cellSize;
        gridLayoutGroup.spacing = spacing;
        gridLayoutGroup.startCorner = startCorner;
        gridLayoutGroup.startAxis = startAxis;
        gridLayoutGroup.childAlignment = childAlignment;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = columnsPerRow;
    }

    [ContextMenu("Create Inventory UI Structure")]
    public void CreateInventoryUIStructure()
    {
        // 인벤토리 UI 구조 자동 생성
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            // Debug.LogError("❌ Canvas를 찾을 수 없습니다! Canvas를 먼저 만들어주세요.");
            return;
        }

        // InventoryPanel 생성
        GameObject inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);
        
        // Panel 컴포넌트 추가
        Image panelImage = inventoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // 반투명 검은색

        // RectTransform 설정 (전체 화면)
        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // SlotParent 생성
        GameObject slotParent = new GameObject("SlotParent");
        slotParent.transform.SetParent(inventoryPanel.transform, false);
        
        // SlotParent RectTransform 설정
        RectTransform slotRect = slotParent.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotRect.anchoredPosition = Vector2.zero;
        slotRect.sizeDelta = new Vector2(400, 300);

        // Grid Layout Group 추가
        GridLayoutGroup grid = slotParent.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columnsPerRow;

        // Content Size Fitter 추가 (선택사항)
        ContentSizeFitter sizeFitter = slotParent.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Debug.Log("🎉 인벤토리 UI 구조가 생성되었습니다!\n" +
        //           "Canvas → InventoryPanel → SlotParent (Grid Layout Group)");
    }

    [ContextMenu("Test Add Dummy Slots")]
    public void TestAddDummySlots()
    {
        // 테스트용 더미 슬롯들 추가
        for (int i = 0; i < 20; i++)
        {
            GameObject slot = new GameObject($"TestSlot_{i + 1}");
            slot.transform.SetParent(transform, false);
            
            // Image 컴포넌트 추가
            Image slotImage = slot.AddComponent<Image>();
            slotImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 회색
            
            // 테두리 추가
            Outline outline = slot.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, 2);
        }
        
        // Debug.Log($"🎮 테스트용 슬롯 20개가 추가되었습니다!");
    }

    // 실시간 Grid 설정 변경 (Inspector에서 값 변경 시 적용)
    void OnValidate()
    {
        if (gridLayoutGroup != null && Application.isPlaying)
        {
            ApplyGridSettings();
        }
    }

    void OnGUI()
    {
        // 화면 좌상단에 간단한 도움말
        GUILayout.BeginArea(new Rect(10, 100, 300, 120));
        GUILayout.Label("=== Grid Layout 도우미 ===");
        GUILayout.Label("G키: Grid Layout 설정");
        GUILayout.Label("우클릭 메뉴:");
        GUILayout.Label("• Setup Grid Layout");
        GUILayout.Label("• Create Inventory UI Structure");
        GUILayout.Label("• Test Add Dummy Slots");
        GUILayout.EndArea();
    }
} 