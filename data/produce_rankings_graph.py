import pandas as pd
import matplotlib.pyplot as plt

file_path = "processed/AllVisualisations_cleaned.csv"  # change this to your file
df = pd.read_csv(file_path, usecols=range(1, 7))
print(df)
df = df[pd.to_numeric(df.iloc[:, 0], errors='coerce').notna()]

df = df.apply(pd.to_numeric)

df.columns = [
    "Arrow",
    "Radar",
    "Lights",
    "Arrow + Radar",
    "Radar + Lights",
    "Arrow + Lights"
]
counts = df.apply(lambda col: col.value_counts().sort_index())


percent = counts.div(counts.sum(axis=0), axis=1)


ax = percent.T.plot(
    kind='bar',
    stacked=True
)


plt.title("Ranking of Visualisations")
plt.xlabel("Visualisation Type")
plt.ylabel("Proportion")
plt.legend(title="Rank (1 = Best)", bbox_to_anchor=(1.05, 1), loc='upper left')
plt.tight_layout()
plt.show()