import React, { Component } from 'react';
import CharacteristicsDataTable from '../../table-tool/components/CharacteristicsDataTable';

interface TableRendererProps {
  data: any;
  meta: any;
}

export class TableRenderer extends Component<TableRendererProps> {
  public render() {
    console.log(this.props.data.result);
    // console.log(this.props.meta);

    const results: any[] = this.props.data.result;

    const attributes: string[] = Object.keys(results[0].attributes);
    // @ts-ignore
    const characteristics: string[] = [
      ...new Set(results.map(result => result.characteristic.name)),
    ];

    const years = results.map(result => result.year).sort();

    // @ts-ignore
    const schoolTypes = [...new Set(results.map(result => result.schoolType))];

    const publicationMeta = this.props.meta;

    return (
      <CharacteristicsDataTable
        attributes={attributes}
        attributesMeta={publicationMeta.attributes}
        characteristics={characteristics}
        characteristicsMeta={publicationMeta.characteristics}
        results={results}
        schoolTypes={schoolTypes}
        years={years}
      />
    );
  }
}
