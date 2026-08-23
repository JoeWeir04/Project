"""
Converts CSV summary-statistics tables into LaTeX tables.

For every .csv file in INPUT_DIR whose header matches EXPECTED_HEADER,
writes a corresponding .tex file into OUTPUT_DIR containing a
\begin{table}...\end{table} block, with columns padded/aligned for readability.

Usage:
    python csv_to_latex.py
    (edit INPUT_DIR / OUTPUT_DIR below, or pass them as command-line args)
"""

import csv
import os
import sys

EXPECTED_HEADER = ["Visualisation", "n", "Mean", "Median", "SD", "Min", "Max", "Range"]


def escape_latex(value: str) -> str:
    """Escape characters that are special in LaTeX (mainly & in e.g. 'Arrow & Radar')."""
    return value.replace("&", r"\&").replace("%", r"\%").replace("_", r"\_")


def make_caption_and_label(filename: str) -> tuple[str, str]:
    """Derive a human-readable caption and a LaTeX label from the filename."""
    stem = os.path.splitext(filename)[0]
    # turn e.g. "distance_to_target_summary" -> "Distance To Target Summary"
    words = stem.replace("_", " ").replace("-", " ").split()
    title = " ".join(w.capitalize() for w in words)
    caption = f"{title} summary statistics by visualisation"
    label = f"tab:{stem}"
    return caption, label


def build_latex_table(rows: list[list[str]], header: list[str], caption: str, label: str) -> str:
    escaped_header = [escape_latex(h) for h in header]
    escaped_rows = [[escape_latex(cell) for cell in row] for row in rows]

    # compute column widths for nice alignment (purely cosmetic)
    all_rows_for_width = [[f"\\textbf{{{h}}}" for h in escaped_header]] + [
        [c + (" \\\\" if i == len(row) - 1 else "") for i, c in enumerate(row)]
        for row in escaped_rows
    ]
    col_count = len(header)
    col_widths = [
        max(len(all_rows_for_width[r][c]) for r in range(len(all_rows_for_width)))
        for c in range(col_count)
    ]

    def format_row(cells: list[str]) -> str:
        padded = [cells[i].ljust(col_widths[i]) for i in range(col_count - 1)]
        padded.append(cells[col_count - 1])  # last column: no trailing padding needed
        return " & ".join(padded) + " \\\\"

    header_line = format_row([f"\\textbf{{{h}}}" for h in escaped_header])
    body_lines = []
    for idx, row in enumerate(escaped_rows):
        line = format_row(row)
        if idx == len(escaped_rows) - 1:
            line = line.rstrip(" \\\\").rstrip()  # no trailing \\ on final row, matches example
        body_lines.append(line)

    col_spec = "l" * col_count

    table = (
        "\\begin{table}[]\n"
        f"\\caption{{{caption}}}\n"
        f"\\label{{{label}}}\n"
        f"\\begin{{tabular}}{{{col_spec}}}\n"
        f"{header_line}\n"
        + "\n".join(body_lines)
        + "\n\\end{tabular}\n"
        "\\end{table}\n"
    )
    return table


def process_folder(input_dir: str, output_dir: str) -> None:
    os.makedirs(output_dir, exist_ok=True)

    csv_files = [f for f in os.listdir(input_dir) if f.lower().endswith(".csv")]
    if not csv_files:
        print(f"No CSV files found in {input_dir}")
        return

    converted = 0
    for filename in sorted(csv_files):
        filepath = os.path.join(input_dir, filename)
        with open(filepath, newline="", encoding="utf-8") as f:
            reader = csv.reader(f)
            try:
                header = next(reader)
            except StopIteration:
                print(f"Skipping {filename}: empty file")
                continue
            rows = [row for row in reader if row]

        if header != EXPECTED_HEADER:
            print(f"Skipping {filename}: header does not match expected columns")
            continue

        caption, label = make_caption_and_label(filename)
        latex = build_latex_table(rows, header, caption, label)

        out_name = os.path.splitext(filename)[0] + ".tex"
        out_path = os.path.join(output_dir, out_name)
        with open(out_path, "w", encoding="utf-8") as f:
            f.write(latex)

        print(f"Converted {filename} -> {out_name}")
        converted += 1

    print(f"\nDone. {converted}/{len(csv_files)} file(s) converted.")


if __name__ == "__main__":
    # Defaults - edit these paths, or pass input/output dirs as command-line args
    INPUT_DIR = "tables"
    OUTPUT_DIR = "tables_latex"

    if len(sys.argv) == 3:
        INPUT_DIR, OUTPUT_DIR = sys.argv[1], sys.argv[2]
    elif len(sys.argv) != 1:
        print("Usage: python csv_to_latex.py [input_dir] [output_dir]")
        sys.exit(1)

    process_folder(INPUT_DIR, OUTPUT_DIR)