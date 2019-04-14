import React, { Component } from 'react';
import CharacteristicsDataTable from '../../table-tool/components/CharacteristicsDataTable';

interface TableRendererProps {
  data: any;
  meta: any;
}

export class TableRenderer extends Component<TableRendererProps> {
  public render() {
    const results: any[] = this.props.data.result;

    const characteristics: string[] = Array.from(
      new Set(results.map(result => result.characteristic.name)),
    );
    const indicators: string[] = Object.keys(results[0].indicators);

    const years = results.map(result => result.timePeriod).sort();

    // @ts-ignore
    const schoolTypes = [...new Set(results.map(result => result.schoolType))];

    const publicationMeta = this.props.meta;

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
}
