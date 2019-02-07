import { FeatureCollection } from 'geojson';
import React, { Component } from 'react';
import Details from '../../components/Details';
import PrototypeMap from './PrototypeMap';
import { Boundaries } from './PrototypeMapBoundaries';

interface PrototypeAbsenceDataState {
  absenceData?: any;
  selectedAuthority: string;
}

interface PrototypeAbsenceDataProps {
  map: (map: PrototypeMap) => void;
}

class PrototypeAbsenceData extends Component<
  PrototypeAbsenceDataProps,
  PrototypeAbsenceDataState
> {
  private map: (map: PrototypeMap) => void;
  private data: FeatureCollection;

  constructor(props: PrototypeAbsenceDataProps) {
    super(props);

    this.map = props.map;

    this.state = {
      absenceData: undefined,
      selectedAuthority: '',
    };

    this.data = PrototypeAbsenceData.preprocessBoundaryData(Boundaries);
  }

  private static preprocessBoundaryData(data: FeatureCollection) {
    const dataParsed = {
      ...data,
      features: data.features.map(g => {
        if (g.properties) {
          g.properties.selectable = g.properties.lad17cd[0] === 'E';
        }

        return g;
      }),
    } as FeatureCollection;

    dataParsed.features.sort((a, b) => {
      const c = [
        a.properties ? a.properties.lad17nm : '',
        b.properties ? b.properties.lad17nm : '',
      ];

      return c[0] < c[1] ? -1 : c[1] > c[0] ? 1 : 0;
    });

    return dataParsed;
  }

  public OnFeatureSelect = (properties: any) => {
    this.setState({
      absenceData: {
        title: properties.lad17nm,
        values: properties.absence,
      },
      selectedAuthority: properties.lad17nm,
    });
  };

  public selectAuthority = (e: any) => {
    const selectedAuthority = e.currentTarget.value;

    this.setState({
      selectedAuthority,
    });
  };

  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third" aria-live="assertive">
          <form>
            <div className="govuk-form-group govuk-!-margin-bottom-6">
              <label
                className="govuk-label govuk-label--s"
                htmlFor="selectedAuthority"
              >
                Select a local authority
              </label>
              <select
                id="selectedAuthority"
                value={this.state.selectedAuthority}
                onChange={this.selectAuthority}
                className="govuk-select"
              >
                <option>Select a local authority</option>
                {this.data.features
                  .filter(
                    feature =>
                      feature.properties && feature.properties.selectable,
                  )
                  .map((feature, idx) => (
                    <option
                      value={
                        feature.properties ? feature.properties.lad17nm : ''
                      }
                      key={feature.properties ? feature.properties.lad17cd : ''}
                    >
                      {feature.properties ? feature.properties.lad17nm : ''}
                    </option>
                  ))}
              </select>
            </div>
          </form>

          {this.state.absenceData ? (
            <div>
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
                  Unauthorised absence is the adipisicing elit. Dolorum hic
                  nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
                </Details>
              </div>
            </div>
          ) : (
            ''
          )}
        </div>
        <div className="govuk-grid-column-two-thirds">
          <PrototypeMap
            Boundaries={Boundaries}
            OnFeatureSelect={this.OnFeatureSelect}
            map={m => this.map(m)}
            selectedAuthority={this.state.selectedAuthority}
          />
        </div>
      </div>
    );
  }
}

export default PrototypeAbsenceData;
