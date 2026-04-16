# LoadTests

Консольный проект нагрузочного тестирования для алгоритмов из `Trees.Core`.

## Что делает

- читает конфигурацию из `appsettings.json`
- строит деревья нужных форм и размеров
- прогоняет выбранные алгоритмы заданное число итераций
- собирает по каждому запуску:
  - время выполнения
  - выделенную память (`GC.GetAllocatedBytesForCurrentThread`)
  - число сборок мусора по поколениям
  - размер результата в цифрах
  - сам результат
  - флаг совпадения с baseline
- сохраняет `csv` и `json` отчёты

## Конфигурация

По умолчанию используется `appsettings.json` в корне проекта.
Можно передать путь вручную:

```bash
dotnet run --project LoadTests -- --config appsettings.json
```

Если `outputDirectory` относительный, он интерпретируется **относительно директории проекта `LoadTests`**, а не `bin/Debug/...`.

## Выходные файлы

Для каждого запуска создаётся отдельная папка вида:

```text
Results/run-YYYYMMDD-HHmmss
```

Внутри:

- `config.snapshot.json`
- `metadata.json`
- `run_records.csv`
- `run_records.json`
- `case_summary.csv`
- `algorithm_summary.csv`

## Python-скрипт анализа

Скрипт лежит в `scripts/analyze_results.py`.

Пример запуска:

```bash
python scripts/analyze_results.py Results
```

Или с явной папкой вывода:

```bash
python scripts/analyze_results.py Results --output-dir AnalysisOutput
```
