from fastapi import FastAPI
import xgboost as xgb
import numpy as np
import random

app = FastAPI()

# טעינת המודל פעם אחת כשהשרת עולה
# חשוב: אנחנו משתמשים ב-Classifier כי אימנת אותו עם objective='binary:logistic'
model = xgb.XGBClassifier()
model.load_model("dda_model.json")

@app.get("/get_wave")
def get_wave():
    num_candidates = 200
    num_features = 10
    candidates = np.zeros((num_candidates, num_features))

    # יצירת 200 האפשרויות
    for i in range(num_candidates):
        candidates[i][0] = random.uniform(2.0, 12.0) # Speed
        candidates[i][1] = random.uniform(0.5, 3.0)  # Interval
        candidates[i][2] = random.randint(1, 5)      # Balls 
        candidates[i][3] = random.uniform(0.1, 1.5)  # Delay
        candidates[i][4] = random.randint(0, 10)     # DistCount
        candidates[i][5] = random.uniform(1.0, 7.0)  # DistSpeed
        candidates[i][6] = random.randint(0, 2)      # CubeMode
        candidates[i][7] = random.uniform(0.5, 1.5)  # Size
        candidates[i][8] = 1.0 if random.random() > 0.5 else 0.0 # ColorShift
        candidates[i][9] = random.uniform(7.0, 12.0) # Distance

    # שימוש ב-predict_proba כדי לקבל את אחוזי ההצלחה
    # העמודה השנייה [:, 1] מייצגת את הסיכוי ל"הצלחה" (Target = 1)
    probs = model.predict_proba(candidates)[:, 1]

    # מציאת הגל שקרוב ביותר ל-60% הצלחה
    best_index = np.argmin(np.abs(probs - 0.60))
    best_wave = candidates[best_index]

    # החזרת הנתונים ליוניטי
    return {
        "ml_BallSpeed": float(best_wave[0]),
        "ml_WaveInterval": float(best_wave[1]),
        "ml_BallsPerWave": int(best_wave[2]),
        "ml_WaveDelay": float(best_wave[3]),
        "ml_DistractionCount": int(best_wave[4]),
        "ml_DistractionSpeed": float(best_wave[5]),
        "ml_CubeMode": int(best_wave[6]),
        "ml_BallSize": float(best_wave[7]),
        "ml_ColorShift": bool(best_wave[8]),
        "ml_SpawnDistance": float(best_wave[9]),
        "expected_success_rate": float(probs[best_index])
    }