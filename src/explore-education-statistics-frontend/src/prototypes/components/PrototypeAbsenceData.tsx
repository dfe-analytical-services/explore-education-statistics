import React, { Component } from 'react';
import Details from '../../components/Details';
import PrototypeMap from './PrototypeMap';

interface PrototypeAbsenceDataState {
  absenceData?: any;
}

class PrototypeAbsenceData extends Component<{}, PrototypeAbsenceDataState> {
  constructor() {
    super({});

    this.state = {
      absenceData: undefined,
    };
  }

  public OnFeatureSelect = (properties: any) => {
    this.setState({
      absenceData: {
        title: properties.lad17nm,
        values: properties.absence,
      },
    });
  };

  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PrototypeMap OnFeatureSelect={this.OnFeatureSelect} />
        </div>
        {this.state.absenceData ? (
          <div className="govuk-grid-column-one-third">
            <span className="govuk-caption-m">Selected local authority</span>
            <h3 className="govuk-heading-m">{this.state.absenceData.title}</h3>
            <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                Overall absence
              </h3>
              <div>
                <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
                  {this.state.absenceData.values.overall}%
                </span>
              </div>
              <Details summary="What does this mean?">
                Overall absence is the adipisicing elit. Dolorum hic nobis
                voluptas quidem fugiat enim ipsa reprehenderit nulla.
              </Details>
            </div>
            <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                Authorised absence
              </h3>
              <div>
                <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
                  {this.state.absenceData.values.authorised}%
                </span>
              </div>
              <Details summary="What does this mean?">
                Overall absence is the adipisicing elit. Dolorum hic nobis
                voluptas quidem fugiat enim ipsa reprehenderit nulla.
              </Details>
            </div>
            <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                Unauthorised absence
              </h3>
              <div>
                <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
                  {this.state.absenceData.values.unauthorised}%
                </span>
              </div>
              <Details summary="What does this mean?">
                Overall absence is the adipisicing elit. Dolorum hic nobis
                voluptas quidem fugiat enim ipsa reprehenderit nulla.
              </Details>
            </div>
          </div>
        ) : (
          ''
        )}
      </div>
    );
  }
}

export default PrototypeAbsenceData;
