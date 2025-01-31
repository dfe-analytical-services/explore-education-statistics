COPY "data" FROM 'data.parquet' (FORMAT 'parquet', CODEC 'ZSTD');
COPY filter_options FROM 'filter_options.parquet' (FORMAT 'parquet', CODEC 'ZSTD');
COPY indicators FROM 'indicators.parquet' (FORMAT 'parquet', CODEC 'ZSTD');
COPY location_options FROM 'location_options.parquet' (FORMAT 'parquet', CODEC 'ZSTD');
COPY time_periods FROM 'time_periods.parquet' (FORMAT 'parquet', CODEC 'ZSTD');
