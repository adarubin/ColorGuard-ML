import random
import numpy as np
import pandas as pd
import xgboost as xgb
from fastapi import FastAPI
from pydantic import BaseModel

# Initialize FastAPI application
app = FastAPI()

# Load the trained XGBoost model
# Ensure 'dda_model.json' is in the same directory as this script
model = xgb.XGBClassifier()
try:
    model.load_model("dda_model.json")
    print("Model loaded successfully.")
except Exception as e:
    print(f"Error loading model: {e}")

# Define the data structure for the response sent to Unity
class WaveData(BaseModel):
    ml_BallSpeed: float
    ml_WaveInterval: float
    ml_BallsPerWave: int
    ml_WaveDelay: float
    ml_DistractionCount: int
    ml_DistractionSpeed: float
    ml_CubeMode: int
    ml_BallSize: float
    ml_ColorShift: bool
    ml_SpawnDistance: float
    expected_success_rate: float

@app.get("/get_wave", response_model=WaveData)
def get_wave():
    # The target success rate we want to maintain for the player (70% success / 30% fail)
    target_success_prob = 0.70 
    
    # Number of random wave configurations to generate and evaluate
    num_candidates = 1000
    
    candidates = []
    
    # 1. Generate 1000 random wave configurations within reasonable gameplay bounds
    for _ in range(num_candidates):
        candidate = {
            "BallSpeed": random.uniform(3.0, 9.0),
            "WaveInterval": random.uniform(1.0, 5.0),
            "BallsPerWave": random.randint(1, 3),
            "WaveDelay": random.uniform(0.6, 3.0),
            "DistractionCount": random.randint(0, 7),
            "DistractionSpeed": random.uniform(2.0, 9.0),
            "CubeMode": random.randint(0, 2),
            "BallSize": random.uniform(0.4, 1.5),
            "ColorShift": random.choice([0, 1]),
            "SpawnDistance": random.uniform(10.0, 16.0)
        }
        candidates.append(candidate)
        
    # Convert the list of dictionaries to a pandas DataFrame for XGBoost prediction
    df_candidates = pd.DataFrame(candidates)
    
    # 2. Predict the probability of success for each candidate
    # predict_proba returns an array of shape (n_samples, n_classes)
    # Class 1 represents "SuccessTarget = 1" (Player wins the wave)
    probabilities = model.predict_proba(df_candidates)[:, 1]
    
    # 3. Find the candidate whose predicted probability is closest to the target (0.70)
    best_index = np.argmin(np.abs(probabilities - target_success_prob))
    best_candidate = candidates[best_index]
    best_prob = float(probabilities[best_index])
    
    # 4. Format and return the best candidate back to Unity
    return WaveData(
        ml_BallSpeed=round(best_candidate["BallSpeed"], 2),
        ml_WaveInterval=round(best_candidate["WaveInterval"], 2),
        ml_BallsPerWave=int(best_candidate["BallsPerWave"]),
        ml_WaveDelay=round(best_candidate["WaveDelay"], 2),
        ml_DistractionCount=int(best_candidate["DistractionCount"]),
        ml_DistractionSpeed=round(best_candidate["DistractionSpeed"], 2),
        ml_CubeMode=int(best_candidate["CubeMode"]),
        ml_BallSize=round(best_candidate["BallSize"], 2),
        ml_ColorShift=bool(best_candidate["ColorShift"]),
        ml_SpawnDistance=round(best_candidate["SpawnDistance"], 2),
        expected_success_rate=round(best_prob, 4)
    )
