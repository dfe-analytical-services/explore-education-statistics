import CharacteristicsDataTable from '@common/modules/table-tool/components/CharacteristicsDataTable';
import {
  CharacteristicsData,
  PublicationMeta,
} from '@common/services/tableBuilderService';
import React from 'react';

export interface TableRendererProps {
  data: CharacteristicsData;
  meta: PublicationMeta;
}

export default function TableRenderer({ meta, data }: TableRendererProps) {
  const results = data.result;

  const characteristics: string[] = Array.from(
    new Set(results.map(result => result.characteristic.name)),
  );
  const indicators: string[] = Object.keys(results[0].indicators);

  const years = results.map(result => result.timePeriod).sort();

  // @ts-ignore
  const schoolTypes = [...new Set(results.map(result => result.schoolType))];

  const publicationMeta = meta;

  return (
    <CharacteristicsDataTable
      characteristics={characteristics}
      characteristicsMeta={publicationMeta.characteristics}
      indicators={indicators}
      indicatorsMeta={publicationMeta.indicators}
      results={results}
      schoolTypes={schoolTypes}
      years={years}
    />
  );
}
