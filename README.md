# AI-Powered Dynamic Difficulty Adjustment (DDA) 2D Game

A fast-paced 2D arcade game built in Unity that utilizes an external machine learning server to dynamically adjust game difficulty in real-time based on player performance.

## Features

* **Real-time ML Integration**: Communicates with a local FastAPI backend running an XGBoost classifier model (`dda_model.json`).
* **Dynamic Difficulty Adjustment (DDA)**: The Python server evaluates 1,000 potential wave configurations using `predict_proba` and selects optimal wave parameters (speed, delay, balls per wave, size, spawn distances) to keep the expected player success rate close to a targeted 80%.
* **Smart Spawning Mechanics**: Integrated anti-overlap memory system ensuring incoming projectiles always spawn from distinct directions.
* **Polished Game Feel ("Juice")**: Smooth visual/audio feedbacks including a custom squashing/stretching animation (`CubeJuice`) on successful hits, pitch-randomized audio for natural combo sounds, and dynamic red screen flashes on failures.
* **Data Collection Loop**: Automatic telemetry logging to a local CSV (`ML_GameData.csv`) file during gameplay, facilitating continuous model retraining and dataset expansion.

## Architecture

The architecture consists of two main components:
1.  **Unity Frontend Client**: Handles the core game loop, player input, sprite rendering, collision detection, and visual effects.
2.  **FastAPI Backend Server**: Exposes a `/get_wave` endpoint that computes the next gameplay variables based on the trained XGBoost model predictions.

## Getting Started

### Prerequisites

* **Unity** 2021.3 LTS or higher.
* **Python** 3.8+
* Python packages: `fastapi`, `uvicorn`, `xgboost`, `numpy`, `pandas`

### Setup & Installation

#### 1. Backend Server Setup

Navigate to the directory containing your Python server and install the required dependencies:

```bash
pip install fastapi uvicorn xgboost numpy pandas
Ensure your trained model dda_model.json is located in the same directory as your main.py script. Run the server:

Bash
python main.py
The FastAPI server will start locally at http://127.0.0.1:8000.

2. Unity Project Setup
Open the Unity project folder via Unity Hub.

Ensure your tags are correctly assigned: Target objects (Left and Right Player Cubes) must be tagged as PlayerCube, and the ball prefabs tagged as Ball.

Check the GameManager in the Inspector to ensure the mainCamera reference is assigned for background flash effects.

Press Play in the editor. The GameManager will automatically query the local backend to spawn the first wave.

Telemetry & Data Logging
Gameplay logs are saved dynamically to the local path:
Application.dataPath + "/ML_GameData.csv"

This continuous stream of structured data contains wave features alongside a binary success target label (SuccessTarget), creating a pipeline for future model iterations.