import React, { Component } from 'react';
import Details from '../../components/Details';
import PrototypeMap from './PrototypeMap';

interface PrototypeAbsenceDataState {
  absenceData?: any;
}

interface PrototypeAbsenceDataProps {
  map: (map: PrototypeMap) => void;
}

class PrototypeAbsenceData extends Component<
  PrototypeAbsenceDataProps,
  PrototypeAbsenceDataState
> {
  private map: (map: PrototypeMap) => void;

  constructor(props: PrototypeAbsenceDataProps) {
    super(props);

    this.map = props.map;

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
          <PrototypeMap
            OnFeatureSelect={this.OnFeatureSelect}
            map={m => this.map(m)}
          />
        </div>
        {this.state.absenceData ? (
          <div className="govuk-grid-column-one-third" aria-live="assertive">
            <span className="govuk-caption-m">Selected local authority</span>
            <h3
              className="govuk-heading-m"
              aria-label={this.state.absenceData.title}
            >
              {this.state.absenceData.title}
            </h3>
            <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                Overall absence
              </h3>
              <p
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                aria-label="Overall absence"
              >
                {this.state.absenceData.values.overall}%
              </p>
              <Details summary="What is overall absence?">
                Overall absence is the adipisicing elit. Dolorum hic nobis
                voluptas quidem fugiat enim ipsa reprehenderit nulla.
              </Details>
            </div>
            <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                Authorised absence
              </h3>
              <p
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                aria-label="Authorised absence"
              >
                {this.state.absenceData.values.authorised}%
              </p>
              <Details summary="What is authorised absence?">
                Authorised absence is the adipisicing elit. Dolorum hic nobis
                voluptas quidem fugiat enim ipsa reprehenderit nulla.
              </Details>
            </div>
            <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                Unauthorised absence
              </h3>
              <p
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                aria-label="Unauthorised absence"
              >
                {this.state.absenceData.values.unauthorised}%
              </p>
              <Details summary="What is unauthorised absence?">
                Unauthorised absence is the adipisicing elit. Dolorum hic nobis
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
