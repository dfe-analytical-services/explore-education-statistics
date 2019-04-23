import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import PrototypeMap from '@common/prototypes/publication/components/PrototypeMap';
import {
  PrototypeMapBoundariesFeature,
  PrototypeMapBoundariesFeatureCollection,
} from '@common/prototypes/publication/components/PrototypeMapBoundaries';
import classNames from 'classnames';
import dynamic from 'next-server/dynamic';
import React, { Component, RefAttributes } from 'react';
import styles from './PrototypeAbsenceData.module.scss';

const DynamicPrototypeMap = dynamic(
  () => import('@common/prototypes/publication/components/PrototypeMap'),
  {
    ssr: false,
  },
);

interface State {
  absenceData?: {
    values: {
      authorised: number;
      overall: number;
      unauthorised: number;
    };
  };
  boundaries?: PrototypeMapBoundariesFeatureCollection;
  data?: PrototypeMapBoundariesFeatureCollection;
  legend?: {
    idx: number;
    max: number;
    min: number;
  }[];
  selectedAuthority: string;
}

class PrototypeAbsenceData extends Component<
  RefAttributes<PrototypeAbsenceData>,
  State
> {
  public state: State = {
    selectedAuthority: '',
  };

  public mapRef: PrototypeMap | null = null;

  public componentDidMount(): void {
    import(
      '@common/prototypes/publication/components/PrototypeMapBoundaries'
    ).then(({ boundaries }) => {
      this.generateLegendData(this.preprocessBoundaryData(boundaries));
    });
  }

  private preprocessBoundaryData(
    data: PrototypeMapBoundariesFeatureCollection,
  ) {
    const dataParsed = {
      ...data,
    };

    dataParsed.features.sort((a, b) => {
      const c = [
        a.properties ? a.properties.lad17nm : '',
        b.properties ? b.properties.lad17nm : '',
      ];

      return c[0] < c[1] ? -1 : c[1] > c[0] ? 1 : 0;
    });

    return dataParsed;
  }

  private generateLegendData(data: PrototypeMapBoundariesFeatureCollection) {
    const minOverall = +data.features.reduce(
      (min, next) =>
        next.properties && next.properties.absence.overall < min
          ? next.properties.absence.overall
          : min,
      100,
    );
    const maxOverall = +data.features.reduce(
      (max, next) =>
        next.properties && next.properties.absence.overall > max
          ? next.properties.absence.overall
          : max,
      0,
    );

    const range = (maxOverall - minOverall) / 4;

    const legend = [4, 3, 2, 1, 0].map(value => {
      return {
        idx: value,
        max: +(minOverall + (value + 1) / range - 0.1).toFixed(1),
        min: +(minOverall + value / range).toFixed(1),
      };
    });

    const parsedData = {
      ...data,
      features: data.features.map(feature => {
        let className = '';

        if (feature.properties.selectable) {
          const rate = Math.floor(
            (feature.properties.absence.overall - minOverall) / range,
          );
          className = styles[`rate${rate}`];
        } else {
          className = styles.unselectable;
        }

        return {
          ...feature,
          properties: {
            ...feature.properties,
            className,
          },
        };
      }),
    };

    this.setState({
      legend,
      data: parsedData,
    });
  }

  public render() {
    const { data, legend } = this.state;

    const localAuthorityOptions = data ? data.features : [];

    return (
      <div className="govuk-grid-row">
        <div
          className={classNames(
            'govuk-grid-column-one-third',
            styles.adjustMobile,
          )}
          aria-live="assertive"
        >
          <form>
            <div className="govuk-form-group govuk-!-margin-bottom-6">
              <FormSelect
                name="selectedAuthority"
                id="selectedAuthority"
                label="Select a local authority"
                value={this.state.selectedAuthority}
                onChange={e => {
                  this.setState({
                    selectedAuthority: e.currentTarget.value,
                  });
                }}
                options={[
                  { label: 'Select a local authority', value: '' },
                ].concat(
                  localAuthorityOptions
                    .filter(
                      feature =>
                        feature.properties && feature.properties.selectable,
                    )
                    .map(feature => ({
                      label: feature.properties
                        ? feature.properties.lad17nm
                        : '',
                      value: feature.properties
                        ? feature.properties.lad17nm
                        : '',
                    })),
                )}
              />
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
                  <span> {this.state.absenceData.values.overall}% </span>
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
                  <span> {this.state.absenceData.values.authorised}% </span>
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
                  <span> {this.state.absenceData.values.unauthorised}% </span>
                </p>
                <Details summary="What is unauthorised absence?">
                  Unauthorised absence is the adipisicing elit. Dolorum hic
                  nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
                </Details>
              </div>
            </div>
          ) : (
            <div />
          )}
          <div className={classNames(styles.legend, styles.hideMobile)}>
            <h3 className="govuk-heading-s">Key to overall absence rate</h3>
            <dl className="govuk-list">
              {legend &&
                legend.map(({ min, max, idx }) => (
                  <dd key={idx}>
                    <span className={styles[`rate${idx}`]}>&nbsp;</span> {min}%
                    to {max}%{' '}
                  </dd>
                ))}
            </dl>
          </div>
        </div>
        <div
          className={classNames(
            'govuk-grid-column-two-thirds',
            styles.hideMobile,
          )}
        >
          {data && (
            <DynamicPrototypeMap
              boundaries={data}
              onFeatureSelect={(
                properties: PrototypeMapBoundariesFeature['properties'],
              ) => {
                if (properties) {
                  this.setState({
                    absenceData: {
                      values: properties.absence,
                    },
                    selectedAuthority: properties.lad17nm,
                  });
                } else {
                  this.setState({
                    absenceData: undefined,
                    selectedAuthority: '',
                  });
                }
              }}
              ref={el => el && (this.mapRef = el)}
              selectedAuthority={this.state.selectedAuthority}
            />
          )}
        </div>
      </div>
    );
  }
}

export default PrototypeAbsenceData;
