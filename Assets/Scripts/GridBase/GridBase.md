# Grid Base

Module này là phần logic grid dùng lại cho các game dạng map ô vuông trong Unity. Code không tạo prefab, sprite, mesh, camera hay input binding. Tầng game/visual tự convert vị trí tay/chuột sang `Vector2Int` rồi gọi API của module.

## Phạm vi

- Quản lý map, ô bị chặn, object và occupancy.
- Load level từ JSON trong `Resources`.
- Kéo object đang có trên map.
- Preview và commit item đặt từ ngoài map.
- Tính shape, rotation, validate va chạm/biên map.
- Trả kết quả đủ dữ liệu để tầng visual tự cập nhật GameObject.

Module cố tình không xử lý world position, cell size, tween, animation hay input system.

## Thành phần chính

- `GridGameModel`: facade chính để load level, kéo object, preview/commit placement.
- `GridMap`: map, blocked cells, object list và occupancy.
- `GridObject`: object runtime trên grid; có thể kế thừa để thêm logic game riêng.
- `GridShapeLibrary`: thư viện shape mặc định và nơi đăng ký shape custom.
- `GridMovementSolver`: di chuyển `Block`, gồm `Free`, `Horizontal`, `Vertical`, `Locked`.
- `SnakeMovementSolver`: kéo đầu/đuôi snake, body tự chạy theo từng bước grid.
- `PlacementSolver`: preview/validate item ngoài map trước khi đặt.
- `GridInteractionService`: wrapper tiện dụng cho pointer down/move/up.

## Quy ước grid

- Tọa độ logic dùng `Vector2Int`.
- `(0,0)` là góc dưới trái theo logic.
- `x` tăng sang phải, `y` tăng lên trên.
- Logic map/occupancy luôn tính theo cell nguyên.
- Tầng visual có thể kéo mượt bằng world position, nhưng khi gọi module này phải truyền cell hiện tại hoặc cell đích đã quy đổi.

## Object types

Hiện module có 2 loại object runtime chính trong level:

1. `Snake`
2. `Block`

`PlaceableItem` là định nghĩa item ngoài map, sau khi đặt thành công sẽ tạo thành `Block`.

### Snake

Snake lưu `cells` theo thứ tự:

```text
index 0 = head
index cuối = tail
```

Chỉ kéo được head hoặc tail. Khi kéo tới target, solver xây path theo các bước ngang/dọc trên grid để body chạy theo. Nếu gặp map edge, blocked cell hoặc object khác, solver dừng ở trạng thái hợp lệ cuối cùng.

Snake không dùng `moveAxis` để giới hạn hướng kéo; hướng đi được quyết định bởi target cell và đường đi ngang/dọc hợp lệ.

### Block

Block là loại đối tượng thứ 2. Block dùng `shapeId`, `pivot`, `rotation`, `moveAxis`.

Khi kéo, solver lấy delta từ cell bắt đầu kéo tới cell hiện tại, lọc theo `moveAxis`, rồi tính `targetPivot = currentPivot + filteredDelta`. Với `Free`, block được di chuyển tự do cả ngang và dọc theo pointer, không bị khóa một trục và không phải đi tuần tự từng ô ở giữa. Solver chỉ validate vị trí đích sau khi đã quy đổi về grid.

Điểm cần phân biệt:

- `Free` nghĩa là tự do theo cả `x` và `y` trong hệ grid.
- `Free` không có nghĩa là object lưu tọa độ float hoặc đứng giữa hai ô.
- Nếu muốn visual bám tay mượt ngoài cell, làm ở tầng visual; state logic cuối cùng vẫn là cell nguyên.

`moveAxis`:

- `Free`: lấy cả delta `x` và `y`, di chuyển ngang/dọc thoải mái trong một lần kéo.
- `Horizontal`: chỉ lấy delta `x`, giữ nguyên `y`.
- `Vertical`: chỉ lấy delta `y`, giữ nguyên `x`.
- `Locked`: không di chuyển.

## Validate di chuyển

Mọi vị trí mới đều được validate bằng `GridMap.ValidateCells`.

Một vị trí không hợp lệ nếu:

- Cell nằm ngoài map.
- Cell nằm trong `blockedCells`.
- Cell bị object khác chiếm.
- Shape sinh ra cell trùng nhau hoặc rỗng.

Với `Block`, preview trả về vị trí đích hợp lệ hoặc không hợp lệ cùng danh sách `InvalidCells`. Solver không sweep từng ô trên đoạn đường đi, nên block có thể "nhảy" qua ô bị chặn nếu vị trí đích cuối cùng hợp lệ. Đây là hành vi hiện tại để object bám thao tác kéo nhanh.

Với `Snake`, solver đi từng bước ngang/dọc và dừng khi bước kế tiếp không hợp lệ.

## Kéo object

Preview trước rồi commit khi thả:

```csharp
model.TryBeginDrag(pointerDownCell);
GridMoveResult preview = model.PreviewActiveDrag(pointerMoveCell);
GridMoveResult final = model.CommitActiveDrag(pointerUpCell);
```

Nếu muốn state logic đổi ngay trong lúc kéo:

```csharp
model.TryBeginDrag(pointerDownCell);
GridMoveResult applied = model.ApplyActiveDrag(pointerMoveCell);
GridMoveResult final = model.CommitActiveDrag(pointerUpCell);
```

Khác biệt chính:

- `PreviewActiveDrag`: chỉ tính kết quả, chưa đổi map.
- `ApplyActiveDrag`: đổi map ngay nếu hợp lệ, sau đó reset start pointer để lần kéo tiếp theo tính delta từ cell mới.
- `CommitActiveDrag`: tính và apply kết quả cuối, rồi clear active drag.
- `CancelActiveDrag`: hủy thao tác kéo hiện tại.

## Placeable item

Item ngoài map nằm trong `placeableItems`. Sau khi `BeginPlacement`, gọi `PreviewPlacement(pivot)` để lấy:

- `Cells`: các ô preview.
- `Success`: vị trí đặt hợp lệ hay không.
- `InvalidCells`: các ô lỗi để visual tô đỏ.
- `Reason`: lý do lỗi nếu không hợp lệ.

Khi thả tay, gọi `CommitPlacement(newObjectId, pivot)`. Nếu thành công, item được thêm vào map như một `Block` mới với shape, rotation và `moveAxis` từ placeable definition.

```csharp
model.BeginPlacement("item_plus");

GridMoveResult preview = model.PreviewPlacement(pointerCell);
GridMoveResult placed = model.CommitPlacement("placed_plus_01", pointerCell);
```

## Shape mặc định

Shape có pivot là một cell integer do shape quy định, không dùng center hình học. Local cells được tính tương đối với pivot.

Shape có sẵn:

- `Cell_1x1`
- `Rect_1x2`
- `Rect_2x2`
- `Rect_1x3`
- `Rect_2x3`
- `Rect_3x3`
- `MissingCorner_2x2`
- `Plus_3x3`

`rotation` dùng giá trị:

- `0`: 0 độ
- `1`: 90 độ clockwise
- `2`: 180 độ
- `3`: 270 độ clockwise

Có thể đăng ký shape custom:

```csharp
GridShapeLibrary library = GridShapeLibrary.CreateDefault();
library.Register(new GridShape("CustomShape", new[]
{
    new Vector2Int(0, 0),
    new Vector2Int(1, 0),
    new Vector2Int(1, 1)
}));

GridGameModel model = new GridGameModel(library);
```

## Load level

Sample JSON nằm tại:

```text
Assets/Resources/GridLevels/sample_01.json
Assets/Resources/GridLevels/sample_02.json
```

Load bằng:

```csharp
GridGameModel model = new GridGameModel();
if (!model.LoadLevelFromResources("GridLevels/sample_01", out GridFailReason reason))
{
    Debug.LogError(reason);
}
```

## Data format

```json
{
  "levelId": "sample",
  "map": {
    "width": 8,
    "height": 8,
    "blockedCells": [
      { "x": 3, "y": 3 }
    ]
  },
  "objects": [
    {
      "id": "snake_01",
      "type": "Snake",
      "shapeId": "",
      "pivot": { "x": 1, "y": 1 },
      "rotation": 0,
      "moveAxis": "Free",
      "cells": [
        { "x": 1, "y": 1 },
        { "x": 1, "y": 2 }
      ]
    },
    {
      "id": "block_free_01",
      "type": "Block",
      "shapeId": "Rect_2x2",
      "pivot": { "x": 4, "y": 1 },
      "rotation": 0,
      "moveAxis": "Free",
      "cells": []
    },
    {
      "id": "block_horizontal_01",
      "type": "Block",
      "shapeId": "Rect_1x2",
      "pivot": { "x": 1, "y": 5 },
      "rotation": 1,
      "moveAxis": "Horizontal",
      "cells": []
    }
  ],
  "placeableItems": [
    {
      "id": "item_plus",
      "shapeId": "Plus_3x3",
      "rotation": 0,
      "moveAxis": "Free"
    }
  ]
}
```

Ghi chú data:

- `type: "Snake"` cần `cells` có ít nhất một cell, không trùng nhau, các cell liền kề nhau theo 4 hướng.
- `type: "Block"` dùng `shapeId`; `cells` có thể để rỗng vì cells được tính từ shape + pivot + rotation.
- `moveAxis` không phân biệt hoa thường khi parse; nếu thiếu thì mặc định là `Free`.
- `placeableItems` không có `pivot`; pivot lấy từ vị trí preview/commit lúc người chơi đặt.

## GridMoveResult

Các API kéo/đặt trả về `GridMoveResult`.

- `Success`: thao tác hợp lệ hay không.
- `Reason`: `GridFailReason.None` nếu thành công, hoặc lý do lỗi.
- `Pivot`: pivot đề xuất/kết quả.
- `Cells`: danh sách cell object chiếm ở kết quả preview/apply/commit.
- `InvalidCells`: các cell lỗi, hữu ích cho preview đỏ.

`GridFailReason` hiện có:

- `InvalidMap`
- `InvalidObject`
- `InvalidShape`
- `InvalidCell`
- `InvalidDragHandle`
- `InvalidDirection`
- `MoveLocked`
- `OutOfBounds`
- `BlockedCell`
- `OccupiedCell`
- `NoValidMove`

## Gắn visual

Tầng visual nên tự quản lý GameObject/prefab. Khi state đổi, gọi:

```csharp
IReadOnlyList<Vector2Int> cells = model.GetObjectCells(objectId);
```

Hoặc đọc `GridMoveResult.Cells` từ preview/apply/commit để update object đang tương tác.

Gợi ý workflow cho kéo mượt:

- Pointer down: convert world position sang cell, gọi `TryBeginDrag`.
- Pointer move: có thể cho GameObject đi theo world position để mượt.
- Đồng thời convert world position sang cell và gọi `PreviewActiveDrag` hoặc `ApplyActiveDrag`.
- Nếu `Success == false`, dùng `InvalidCells`/`Reason` để hiển thị lỗi.
- Pointer up: gọi `CommitActiveDrag`, rồi snap visual về `Cells`/`Pivot` logic trả về.

Nếu object là `Block` có `moveAxis: Free`, visual có thể kéo ngang dọc thoải mái theo tay; logic module chỉ chốt vị trí hợp lệ theo cell khi preview/apply/commit.
