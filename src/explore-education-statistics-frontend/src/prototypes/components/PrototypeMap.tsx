import React, {
  ChangeEvent,
  Component,
  FormEvent,
  ReactEventHandler,
} from 'react';

import 'leaflet/dist/leaflet.css';

import styles from './PrototypeMap.module.scss';

import { Boundaries } from './PrototypeMapBoundaries';

import { Feature, FeatureCollection } from 'geojson';

import { LatLngBounds, Path } from 'leaflet';
import { GeoJSON, Map } from 'react-leaflet';

export interface PrototypeMapProps {
  OnFeatureSelect?: any;
  map: (map: PrototypeMap) => void;
}

interface PrototypeMapState {
  selectedAuthority: string;
  selectedFeature?: Feature;
}

class PrototypeMap extends Component<PrototypeMapProps, PrototypeMapState> {
  private mapNode: any;

  public static defaultProps: Partial<PrototypeMapProps> = {
    OnFeatureSelect: undefined,
  };

  private data: FeatureCollection;
  private OnFeatureSelect: any | undefined;
  private legend: any;
  private geoRef: any;

  constructor(props: PrototypeMapProps) {
    super(props);

    if (props.map) props.map(this);

    this.state = {
      selectedAuthority: '',
      selectedFeature: undefined,
    };

    /**
     * lad17cd - code
     * lad17nm - name
     */

    this.data = {
      ...Boundaries,
      features: Boundaries.features.map(g => {
        if (g.properties) {
          g.properties.selectable = g.properties.lad17cd[0] === 'E';
        }

        return g;
      }),
    } as FeatureCollection;

    const minOverall = +this.data.features.reduce(
      (min, next) =>
        next.properties && next.properties.absence.overall < min
          ? next.properties.absence.overall
          : min,
      100,
    );
    const maxOverall = +this.data.features.reduce(
      (max, next) =>
        next.properties && next.properties.absence.overall > max
          ? next.properties.absence.overall
          : max,
      0,
    );

    const range = (maxOverall - minOverall) / 5;

    this.legend = [4, 3, 2, 1, 0].map(value => {
      return {
        max: (minOverall + (value + 1) / range - 0.1).toFixed(1),
        min: (minOverall + value / range).toFixed(1),
      };
    });

    this.data.features = this.data.features.map(feature => {
      if (feature.properties) {
        if (feature.properties.selectable) {
          const rate = Math.trunc(
            (feature.properties.absence.overall - minOverall) / range,
          );
          feature.properties.className = styles[`rate${rate}`];
        } else {
          feature.properties.className = styles.unselectable;
        }
      }

      return feature;
    });

    this.data.features.sort((a, b) => {
      const c = [
        a.properties ? a.properties.lad17nm : '',
        b.properties ? b.properties.lad17nm : '',
      ];

      return c[0] < c[1] ? -1 : c[1] > c[0] ? 1 : 0;
    });

    this.OnFeatureSelect = this.props.OnFeatureSelect;
  }

  public refresh() {
    this.mapNode.leafletElement.invalidateSize();
  }

  private onEachFeature = (feature: Feature, layer: Path) => {
    if (feature.properties && feature.properties.selectable) {
      const featureIndex = this.data.features.findIndex(f => f === feature);

      // @ts-ignore
      this.data.features[featureIndex].properties.layer = layer;

      // @ts-ignore
      layer.bindTooltip(feature.properties.lad17nm, {
        // className: f.properties.toolTipClass,
        direction: 'auto',
        opacity: 1.0,
        sticky: true,
      });

      layer.setStyle({ weight: 1, opacity: 1.0 });
    }
  };

  private selectFeature = (feature?: Feature) => {
    // @ts-ignore
    if (feature.properties.selectable) {
      if (this.state.selectedAuthority !== '') {
        // @ts-ignore
        const currentSelectedLayer = this.state.selectedFeature.properties
          .layer;

        currentSelectedLayer.getElement().classList.remove(styles.selected);
      }

      if (this.OnFeatureSelect && feature && feature.properties) {
        this.OnFeatureSelect(feature.properties);
      }

      if (feature !== undefined) {
        // @ts-ignore
        const selectedLayer = feature.properties.layer;

        selectedLayer.getElement().classList.add(styles.selected);

        this.mapNode.leafletElement.fitBounds(selectedLayer.getBounds(), {
          padding: [200, 200],
        });
        selectedLayer.bringToFront();

        this.setState({
          // @ts-ignore
          selectedAuthority: feature.properties.lad17nm,
          selectedFeature: feature,
        });
      } else {
        this.setState({
          selectedAuthority: '',
          selectedFeature: undefined,
        });

        this.mapNode.leafletElement.fitBounds(
          this.geoRef.leafletElement.getBounds(),
        );
      }
    }
  };

  // @ts-ignore
  private click = (e: any) => {
    if (e.sourceTarget.feature) {
      this.selectFeature(e.sourceTarget.feature);
    }
  };

  private selectAuthority = (e: ChangeEvent<HTMLSelectElement>) => {
    const selectedFeatureName = e.currentTarget.value;

    const feature = this.data.features.find(
      // @ts-ignore
      f => f.properties.lad17nm === selectedFeatureName,
    );

    this.selectFeature(feature);
  };

  private styleFeature = (f: any) => {
    return { className: f.properties && f.properties.className };
  };

  public componentDidMount() {
    // force a refresh to fix a bug
    requestAnimationFrame(() => this.refresh());

    //    const bounds:LatLngBounds = this.geoRef.getBounds();
  }

  public render() {
    const position = {
      lat: 53.009865,
      lng: -3.2524038,
    };

    const bounds = new LatLngBounds(
      { lat: 48, lng: -6.5 },
      { lat: 60, lng: 2 },
    );

    return (
      <div>
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
                    value={feature.properties ? feature.properties.lad17nm : ''}
                    key={feature.properties ? feature.properties.lad17cd : ''}
                  >
                    {feature.properties ? feature.properties.lad17nm : ''}
                  </option>
                ))}
            </select>
          </div>
        </form>
        <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
          Or choose a region on the map below to show pupil absence figures by
          local authority
        </h3>
        <Map
          ref={(n: any) => (this.mapNode = n)}
          center={position}
          className={styles.map}
          zoom={6.5}
          minZoom={6.5}
          zoomSnap={0.5}
        >
          <GeoJSON
            ref={(geo: any) => (this.geoRef = geo)}
            data={this.data}
            onEachFeature={this.onEachFeature}
            style={this.styleFeature}
            onClick={this.click}
          />
        </Map>
        <div className={styles.legend}>
          <h3 className="govuk-heading-s">Key to overall absence rate</h3>
          <dl className="govuk-list">
            {this.legend.map(({ min, max }: any, idx: number) => (
              <dd key={idx}>
                <span className={styles[`rate${idx}`]}>&nbsp;</span> {min}% to{' '}
                {max}%{' '}
              </dd>
            ))}
          </dl>
        </div>
      </div>
    );
  }
}

export default PrototypeMap;
