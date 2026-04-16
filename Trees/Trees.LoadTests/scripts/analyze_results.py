#!/usr/bin/env python3
import argparse
from datetime import datetime
from pathlib import Path

import matplotlib.pyplot as plt
import pandas as pd

# python3 scripts/analyze_results.py Results

COLD_ALGORITHMS = ["RecursiveDfs", "Bfs", "CacheDfs", "Morris"]

HOT_COMPARISON_ORDER = [
    ("RecursiveDfs", "WarmBaseline"),
    ("Bfs", "Cold"),
    ("CacheDfs", "WarmCache"),
    ("Morris", "Cold"),
]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Analyze load test results and generate summaries/plots.")
    parser.add_argument("results_dir", help="Path to a run directory or a parent directory containing multiple run directories.")
    parser.add_argument("--output-dir", dest="output_dir", help="Directory for analysis outputs. Defaults to <project>/Analysis/<timestamp>.")
    return parser.parse_args()


def discover_run_csv_files(results_dir: Path) -> list[Path]:
    if not results_dir.exists():
        raise FileNotFoundError(f"Results directory does not exist: {results_dir}")

    direct_csv = results_dir / "run_records.csv"
    if direct_csv.exists():
        return [direct_csv]

    return sorted(results_dir.rglob("run_records.csv"))


def ensure_output_dir(explicit_output_dir: str | None) -> Path:
    if explicit_output_dir:
        output_dir = Path(explicit_output_dir).resolve()
    else:
        project_root = Path(__file__).resolve().parents[1]
        output_dir = project_root / "Analysis" / datetime.now().strftime("analysis-%Y%m%d-%H%M%S")

    output_dir.mkdir(parents=True, exist_ok=True)
    return output_dir


def load_frames(csv_files: list[Path]) -> pd.DataFrame:
    frames = []
    for csv_file in csv_files:
        frame = pd.read_csv(csv_file)
        frame["source_run_directory"] = str(csv_file.parent)
        frames.append(frame)

    if not frames:
        raise ValueError("No run_records.csv files were found.")

    data = pd.concat(frames, ignore_index=True)
    data["StartedAtUtc"] = pd.to_datetime(data["StartedAtUtc"], utc=True, errors="coerce")
    if "Scenario" not in data.columns:
        data["Scenario"] = "Cold"
    return data


def make_dir(path: Path) -> Path:
    path.mkdir(parents=True, exist_ok=True)
    return path


def line_label(algorithm: str, scenario: str) -> str:
    return f"{algorithm} ({scenario})"


def hot_comparison_frame(data: pd.DataFrame) -> pd.DataFrame:
    return data[
        ((data["Algorithm"] == "RecursiveDfs") & (data["Scenario"] == "WarmBaseline")) |
        ((data["Algorithm"] == "Bfs") & (data["Scenario"] == "Cold")) |
        ((data["Algorithm"] == "CacheDfs") & (data["Scenario"] == "WarmCache")) |
        ((data["Algorithm"] == "Morris") & (data["Scenario"] == "Cold"))
        ]


def write_summary_csvs(data: pd.DataFrame, output_dir: Path) -> None:
    data.to_csv(output_dir / "all_runs.csv", index=False)

    case_summary = (
        data.groupby(["Scenario", "Shape", "Size", "Algorithm"], as_index=False)
        .agg(
            SampleCount=("ElapsedMilliseconds", "count"),
            MeanMilliseconds=("ElapsedMilliseconds", "mean"),
            MedianMilliseconds=("ElapsedMilliseconds", "median"),
            StdMilliseconds=("ElapsedMilliseconds", "std"),
            MinMilliseconds=("ElapsedMilliseconds", "min"),
            MaxMilliseconds=("ElapsedMilliseconds", "max"),
            MeanAllocatedBytes=("AllocatedBytes", "mean"),
            MedianAllocatedBytes=("AllocatedBytes", "median"),
            MinAllocatedBytes=("AllocatedBytes", "min"),
            MaxAllocatedBytes=("AllocatedBytes", "max"),
            Failures=("MatchesBaseline", lambda s: (~s.astype(bool)).sum())
        )
        .sort_values(["Scenario", "Shape", "Size", "Algorithm"])
    )
    case_summary.to_csv(output_dir / "case_summary_merged.csv", index=False)

    algorithm_summary = (
        data.groupby(["Scenario", "Algorithm"], as_index=False)
        .agg(
            SampleCount=("ElapsedMilliseconds", "count"),
            MeanMilliseconds=("ElapsedMilliseconds", "mean"),
            MedianMilliseconds=("ElapsedMilliseconds", "median"),
            StdMilliseconds=("ElapsedMilliseconds", "std"),
            MeanAllocatedBytes=("AllocatedBytes", "mean"),
            MedianAllocatedBytes=("AllocatedBytes", "median"),
            Failures=("MatchesBaseline", lambda s: (~s.astype(bool)).sum())
        )
        .sort_values(["Scenario", "Algorithm"])
    )
    algorithm_summary.to_csv(output_dir / "algorithm_summary_merged.csv", index=False)

    hot_compare = hot_comparison_frame(data)
    if not hot_compare.empty:
        hot_summary = (
            hot_compare.groupby(["Shape", "Size", "Algorithm", "Scenario"], as_index=False)
            .agg(
                SampleCount=("ElapsedMilliseconds", "count"),
                MeanMilliseconds=("ElapsedMilliseconds", "mean"),
                MedianMilliseconds=("ElapsedMilliseconds", "median"),
                StdMilliseconds=("ElapsedMilliseconds", "std"),
                MeanAllocatedBytes=("AllocatedBytes", "mean"),
                MedianAllocatedBytes=("AllocatedBytes", "median")
            )
            .sort_values(["Shape", "Size", "Algorithm", "Scenario"])
        )
        hot_summary.to_csv(output_dir / "hot_comparison_summary.csv", index=False)


def plot_cold_lines_by_shape(data: pd.DataFrame, metric: str, output_dir: Path):
    plot_dir = make_dir(output_dir / "cold_lines")
    frame = data[data["Scenario"] == "Cold"]
    summary = (
        frame.groupby(["Shape", "Size", "Algorithm"], as_index=False)[metric]
        .mean()
        .sort_values(["Shape", "Algorithm", "Size"])
    )

    for shape, shape_frame in summary.groupby("Shape"):
        plt.figure(figsize=(12, 6))
        ordered = [alg for alg in COLD_ALGORITHMS if alg in set(shape_frame["Algorithm"])]
        for algorithm in ordered:
            line = shape_frame[shape_frame["Algorithm"] == algorithm].sort_values("Size")
            if line.empty:
                continue
            plt.plot(line["Size"], line[metric], marker="o", label=f"{algorithm} (Cold)")

        plt.title(f"Cold {metric} by size - {shape}")
        plt.xlabel("Size")
        plt.ylabel(metric)
        plt.legend()
        plt.tight_layout()
        plt.savefig(plot_dir / f"{str(shape).lower()}_{metric.lower()}.png", dpi=170)
        plt.close()


def plot_hot_lines_by_shape(data: pd.DataFrame, metric: str, output_dir: Path):
    plot_dir = make_dir(output_dir / "hot_lines")
    frame = hot_comparison_frame(data)

    if frame.empty:
        return

    summary = (
        frame.groupby(["Shape", "Size", "Algorithm", "Scenario"], as_index=False)[metric]
        .mean()
        .sort_values(["Shape", "Algorithm", "Scenario", "Size"])
    )

    for shape, shape_frame in summary.groupby("Shape"):
        plt.figure(figsize=(12, 6))
        for algorithm, scenario in HOT_COMPARISON_ORDER:
            line = shape_frame[
                (shape_frame["Algorithm"] == algorithm) &
                (shape_frame["Scenario"] == scenario)
                ].sort_values("Size")

            if line.empty:
                continue

            plt.plot(line["Size"], line[metric], marker="o", label=line_label(algorithm, scenario))

        plt.title(f"Hot {metric} by size - {shape}")
        plt.xlabel("Size")
        plt.ylabel(metric)
        plt.legend()
        plt.tight_layout()
        plt.savefig(plot_dir / f"{str(shape).lower()}_{metric.lower()}.png", dpi=170)
        plt.close()


def boxplot_for_shape_and_size(
        data: pd.DataFrame,
        metric: str,
        output_dir: Path,
        *,
        filter_fn,
        filename_prefix: str,
        title_prefix: str,
        order_labels: list[tuple[str, str]]
):
    plot_dir = make_dir(output_dir / filename_prefix)
    frame = filter_fn(data.copy())

    for (shape, size), part in frame.groupby(["Shape", "Size"]):
        labels = []
        values = []

        for algorithm, scenario in order_labels:
            series = part[
                (part["Algorithm"] == algorithm) &
                (part["Scenario"] == scenario)
                ][metric].dropna()

            if series.empty:
                continue

            labels.append(line_label(algorithm, scenario))
            values.append(series.values)

        if not values:
            continue

        plt.figure(figsize=(13, 7))
        plt.boxplot(values, tick_labels=labels)
        plt.title(f"{title_prefix} - {shape} - size {size}")
        plt.ylabel(metric)
        plt.xticks(rotation=20)
        plt.tight_layout()
        plt.savefig(plot_dir / f"{str(shape).lower()}_size_{size}_{metric.lower()}.png", dpi=170)
        plt.close()


def main() -> None:
    args = parse_args()
    results_dir = Path(args.results_dir).resolve()
    csv_files = discover_run_csv_files(results_dir)
    output_dir = ensure_output_dir(args.output_dir)

    data = load_frames(csv_files)

    write_summary_csvs(data, output_dir)

    plot_cold_lines_by_shape(data, "ElapsedMilliseconds", output_dir)
    plot_cold_lines_by_shape(data, "AllocatedBytes", output_dir)

    plot_hot_lines_by_shape(data, "ElapsedMilliseconds", output_dir)
    plot_hot_lines_by_shape(data, "AllocatedBytes", output_dir)

    boxplot_for_shape_and_size(
        data,
        "ElapsedMilliseconds",
        output_dir,
        filter_fn=lambda d: d[d["Scenario"] == "Cold"],
        filename_prefix="cold_boxplots_time",
        title_prefix="Cold elapsed milliseconds",
        order_labels=[(alg, "Cold") for alg in COLD_ALGORITHMS]
    )

    boxplot_for_shape_and_size(
        data,
        "AllocatedBytes",
        output_dir,
        filter_fn=lambda d: d[d["Scenario"] == "Cold"],
        filename_prefix="cold_boxplots_allocated",
        title_prefix="Cold allocated bytes",
        order_labels=[(alg, "Cold") for alg in COLD_ALGORITHMS]
    )

    boxplot_for_shape_and_size(
        data,
        "ElapsedMilliseconds",
        output_dir,
        filter_fn=hot_comparison_frame,
        filename_prefix="hot_boxplots_time",
        title_prefix="Hot elapsed milliseconds",
        order_labels=HOT_COMPARISON_ORDER
    )

    boxplot_for_shape_and_size(
        data,
        "AllocatedBytes",
        output_dir,
        filter_fn=hot_comparison_frame,
        filename_prefix="hot_boxplots_allocated",
        title_prefix="Hot allocated bytes",
        order_labels=HOT_COMPARISON_ORDER
    )

    print(f"Analysis saved to: {output_dir}")


if __name__ == "__main__":
    main()
    