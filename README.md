# RL Tank Defense Game
This project was developed by Jeremy Gerster on 14 Dec 2024 as part of the course: *CE6127 Artificial Intelligence in Game Design*, NTU Singapore. The goal of the project was to develop a Deep Reinforcement Learning (DRL) Tank Defense Game as well analyze the training progression and tune the various hyperparameters to improve training performance. The project is based on https://github.com/sascharo/24s1-ce6127-ml-r21-asgmt

---

## Demo:



---

## Setup

### Installing PyTorch
```bash
# for macOS
pip install torch torchvision torchaudio
```

### Clone ML-Agents and Installing Environment Packages 

- Clone (or download and unpack) the latest ML-Agents (>=R21)
from: https://github.com/Unity-Technologies/ml-agents.git

- In ml-agents-develop run (for training only):
```bash
python -m pip install -e ./ml-agents-envs
python -m pip install -e ./ml-agents
```

Then open RLTankDefense in unity

### Parameters and Inference
- Parameter configs for RL can be found in RLTankDefense/configs/tank-configs.yaml
- 

### Training
Run inside RLTankDefense:<br>
```bash
mlagents-learn configs/tank-configs.yaml 
```
Then press play and wait for it to train.

---

## Game Rules
<p align="center"><img src="sources_readme/game.png" alt="Metrics" width="500"></p><br>  

The AI tank moves horizontally at the bottom of the field and shoots vertically with raycast. The goal is to reach 20 points.  

- **Blue tanks (enemies):** +2 reward for shooting, –3 for colliding, can fly.  
- **Yellow tanks (friendlies):** –1 reward for shooting, +1 for colliding.  
- **Missiles:** +1 reward for shooting, –10 for colliding.  
- **Game ends** after at least 20 points or on collision with a blue tank/missile.  

---

## Project Progression
The project was developed in three stages:  

1. **Basic version** – initial setup, Markov Decision Process definition, simple heuristics.  
2. **Optimized version** – parallel training with multiple environments, hyperparameter tuning to meet baseline performance.  
3. **Final version** – extended gameplay with vertical shooting, flying enemies, and homing missiles, adding significant complexity to the agent’s strategy.  


## Training Progression

The agent was first trained with flying tanks and later extended with homing missiles.  
Initial runs showed unstable learning: cumulative rewards fluctuated around -10, episode lengths varied widely, and policy updates were inconsistent. This indicated difficulties in stable exploration and reward prediction.  

<p align="center">
  <img src="sources_readme/init_training1.png" alt="init_training1" width="250">
  <img src="sources_readme/init_training2.png" alt="init_training2" width="250">
</p>

To improve stability, several adjustments were made: a slightly lower learning rate, larger batch and buffer sizes, reduced network depth, and higher curiosity strength. Entropy and reward signals were also tuned to encourage exploration and stabilize updates.  

<p align="center"><img src="plots/final_configs.png" alt="final_configs" width="500"></p>

With these changes, the final version showed higher cumulative rewards, longer and more stable episodes, lower curiosity losses, and reduced policy loss, which enabled the agent to consistently win.  

<p align="center">
  <img src="sources_readme/final_training1.png" alt="final_training1" width="250">
  <img src="sources_readme/final_training2.png" alt="final_training2" width="250">
</p>
