# UGV ROS Unity Publishers

Unity Package providing ROS 2 publishers for an Unmanned Ground Vehicle (UGV), including:

- TF (`/tf`)
- LaserScan (LiDAR)
- Camera Image
- Vehicle feedback & joint states

This package is distributed as a **Unity Package Manager (UPM) package**, not a `.unitypackage` and not a full Unity project.

---

## Requirements

- **Unity**: 2022.3 LTS (tested on 2022.3.6f1)
- **ROS TCP Connector**: 0.7.0
- **ROS 2** (Humble / Iron tested)

---

## Installation

### Step 1 — Install ROS TCP Connector (required)

This package depends on **ROS TCP Connector**, which must be installed **at the Unity project level**.

Add the following line to your Unity project’s  
`Packages/manifest.json`:

```json
"com.unity.robotics.ros-tcp-connector":
  "https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector#v0.7.0"
```

> This is required because Unity Package Manager does not allow Git dependencies to be resolved automatically from another package.

Official docs for dependency:
https://github.com/Unity-Technologies/ROS-TCP-Connector

---

## Step 2 — Add this package

## Option A: Add from Git URL (recommended)

In Unity:

```
Window → Package Manager → +
→ Add package from git URL…
```

Enter:

```
https://github.com/<your-org-or-username>/com.yusufa.ugv-ros-publishers.git
```

(Optional, recommended):

```
https://github.com/<your-org-or-username>/com.yusufa.ugv-ros-publishers.git#v0.1.0
```

Option B: Add from disk (local development)
```
Package Manager → +
→ Add package from disk…
→ select package.json
```

---

## Demo / Sample Content

This package includes optional demo content provided as a Unity Package Sample.

After installing the package:

1. Open Package Manager

2. Select UGV ROS Unity Publishers

3. Scroll to Samples

4. Click Import on UGV Demo

This will import:

* Example scene

* Vehicle prefab

* LiDAR and camera setup

* Example ROS publishers wired and ready

The demo content is located under:

```
Samples~/UGV_Demo/
```

---

## Folder Structure (Overview)

```
com.yusufa.ugv-ros-publishers/
├── Runtime/
│   ├── Scripts/
│   ├── RosMessages/
│   └── Resources/
├── Samples~/UGV_Demo/
│   ├── Scenes/
│   ├── Prefabs/
│   └── Models/
├── Documentation~/
│   └── Msgs/
├── package.json
└── README.md
```

* Runtime/ — required code & assets

* Samples~/ — optional demo content (importable)

* Documentation~/ — ROS .msg source definitions

---

## Notes on Dependencies

* This package does not automatically install ROS TCP Connector

* This is a deliberate design choice to avoid Unity Package Manager dependency resolution issues

* Users must install ROS TCP Connector once per project (see Step 1)

---

## License

MIT (or update as appropriate)

---

## Author

**Yusuf A**

---

## Known Limitations

Odometry visualization may use demo-specific techniques (see code comments)

---

## Support

If you encounter issues:

Verify ROS TCP Connector is installed

Check Unity version compatibility

Ensure ROS bridge is running before Play

