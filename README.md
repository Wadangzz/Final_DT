# 🎮 Final_DT
![image](https://github.com/user-attachments/assets/07918e17-1427-4638-93a5-cf57322e74ec)


## 📘 프로젝트 개요

**Final_DT**는 Unity 기반의 스마트팩토리 디지털 트윈 시뮬레이터로,  
**PLC에서 Modbus TCP로 수신한 데이터를 기반으로 Unity 오브젝트를 제어**하고,  
**충돌 이벤트를 통해 오브젝트 생성/삭제, 카메라 시점 변경**을 구현한 **Digital Twin 프로젝트**입니다.

---

## 🔧 주요 기능

### 🧩 1. Modbus TCP 연동 (NModbus)
- Unity에서 **NModbus 패키지**를 활용하여 **Modbus Master**로 동작
- PLC_NModbus(서버)에서 데이터를 읽어 **Unity 내부 GameObject의 상태를 실시간으로 제어**
- `coils`, `registers`, `joints`, `servo`, `cylinder` 등의 값 반영

### 🧠 2. 실시간 시뮬레이션 연동
- **ServoTower 위치 이동**, **Cylinders 동작**, **Robot 관절 각도(joint) 제어**
- 오브젝트 위치, 속도, 상태가 실시간 Modbus 값에 따라 업데이트됨

### 🎯 3. 충돌 기반 이벤트 트리거
- 특정 위치에서 충돌(`Collider Trigger`) 발생 시:
  - 특정 위치에 **새로운 Object 생성**
  - 흡착, 그리퍼 **Pick&Place 구현**

---

## 🗂️ 주요 구성 요소(Assets/Script)

| 구성 요소 | 설명 |
|-----------|------|
| `DT_TEST.cs` | Modbus TCP 연동 및 전체 오브젝트 제어 로직 담당 |
| `ConveyorItem.cs` | 이동 대상 오브젝트의 충돌, 삭제 조건 처리 |
| `ObjectReset.cs` | 특정 위치에서 셀, 캡 등 오브젝트 자동 제거 |
| `CameraControl.cs` | 조건에 따른 카메라 시점 전환 |
| `NModbus.dll` | Unity C#에서 Modbus TCP 통신을 위한 .NET 라이브러리 |
| FBX Assets | Conveyor, Robot, Cap 등 실제 라인 구성 요소의 3D 모델 |

---

## 🔄 시스템 동작 요약

| **단계**                            | **동작 내용** |
|------------------------------------|----------------|
| **Modbus Slave 서버에서 데이터 읽기** | PLC 상태값(Coils, Registers 등) 수신 |
| **PLC 상태 반영**                  | 수신한 값을 기반으로 Unity 내 제어 로직 반영 |
| **Unity 오브젝트 실시간 제어**      | 실린더, 서보, 로봇 등의 위치·회전 제어 |
| **충돌 발생**                      | Trigger(Collider) 이벤트 감지 |
| **오브젝트 동작 처리**             | `Cell`, `Case`, `Cap` 오브젝트 삭제 및 새 오브젝트 생성 |
