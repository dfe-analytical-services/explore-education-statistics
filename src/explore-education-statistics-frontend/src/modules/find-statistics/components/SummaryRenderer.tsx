import * as React from 'react';
import { PublicationMeta } from '../../../services/tableBuilderService';

interface Props {
  data: any;
  meta: PublicationMeta;
  dataKeys: string[];
  description: { type: string; body: string };
}

export class SummaryRenderer extends React.Component<Props> {
  public render() {
    const result = this.props.data.result;

    const latest = result[result.length - 1];
    const indicators = latest.indicators;

    const dataKeys = this.props.dataKeys;
    const indicatorMeta = Array.prototype
      .concat(...Object.values(this.props.meta.indicators))
      .reduce((allMeta, next) => ({ ...allMeta, [next.name]: next }), {});

    return (
      <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
        {dataKeys.map(key => (
          <div className="dfe-dash-tiles__tile" key={key}>
            <h3 className="govuk-heading-m dfe-dash-tiles__heading">
              {indicatorMeta[key].label}
            </h3>
            <p className="govuk-heading-xl govuk-!-margin-bottom-2">
              {indicators[key]}
              {indicatorMeta[key].unit}
            </p>
          </div>
        ))}
      </div>
    );
  }
}
