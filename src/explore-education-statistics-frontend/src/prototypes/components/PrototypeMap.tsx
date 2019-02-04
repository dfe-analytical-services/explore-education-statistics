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

import { Path } from 'leaflet';
import { GeoJSON, Map } from 'react-leaflet';

export interface PrototypeMapProps {
  OnFeatureSelect?: any;
  map: (map: PrototypeMap) => void;
}

interface PrototypeMapState {
  selectedAuthority: number;
  selectedLayer: any;
}

class PrototypeMap extends Component<PrototypeMapProps, PrototypeMapState> {
  private mapNode: any;

  public static defaultProps: Partial<PrototypeMapProps> = {
    OnFeatureSelect: undefined,
  };

  private data: FeatureCollection;
  private OnFeatureSelect: any | undefined;

  constructor(props: PrototypeMapProps) {
    super(props);

    if (props.map) props.map(this);

    this.state = {
      selectedAuthority: -1,
      selectedLayer: undefined,
    };

    /**
     * lad17cd - code
     * lad17nm - name
     */

    this.data = {
      ...Boundaries,
      features: Boundaries.features.filter(g => {
        return g.properties !== null && g.properties.lad17cd[0] === 'E';
      }),
    } as FeatureCollection;

    const minOverall = this.data.features.reduce(
      (min, next) =>
        next.properties && next.properties.absence.overall < min
          ? next.properties.absence.overall
          : min,
      100,
    );
    const maxOverall = this.data.features.reduce(
      (max, next) =>
        next.properties && next.properties.absence.overall > max
          ? next.properties.absence.overall
          : max,
      0,
    );

    const range = (maxOverall - minOverall) / 5;

    this.data.features = this.data.features.map(feature => {
      if (feature.properties) {
        const rate = Math.trunc(
          (feature.properties.absence.overall - minOverall) / range,
        );
        feature.properties.className = styles[`rate${rate}`];
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
    const featureIndex = this.data.features.findIndex(f => f === feature);

    // @ts-ignore
    this.data.features[featureIndex].properties.layer = layer;

    // @ts-ignore
    layer.bindTooltip(feature.properties.lad17nm, {
      // className: f.properties.toolTipClass,
      direction: 'center',
      opacity: 1.0,
    });

    layer.setStyle({ weight: 1 });
  };

  private selectFeature = (feature: Feature) => {
    if (this.state.selectedAuthority !== -1) {
      // @ts-ignore
      this.data.features[this.state.selectedAuthority].properties.layer
        .getElement()
        .classList.remove(styles.selected);
    }

    const featureIndex = this.data.features.findIndex(f => f === feature);

    this.setState({
      selectedAuthority: featureIndex,
    });

    if (this.OnFeatureSelect && feature.properties) {
      this.OnFeatureSelect(feature.properties);
    }

    if (featureIndex !== -1) {
      // @ts-ignore
      this.data.features[featureIndex].properties.layer
        .getElement()
        .classList.add(styles.selected);
    }
  };

  // @ts-ignore
  private click = (e: any) => {
    if (e.sourceTarget.feature) {
      this.selectFeature(e.sourceTarget.feature);
    }
  };

  private selectAuthority = (e: ChangeEvent<HTMLSelectElement>) => {
    const feature = this.data.features[parseInt(e.currentTarget.value, 10)];
    this.selectFeature(feature);
  };

  private styleFeature = (f: any) => {
    return { className: f.properties && f.properties.className };
  };

  public componentDidMount() {
    // force a refresh to fix a bug
    requestAnimationFrame(() => this.refresh());
  }

  public render() {
    const position = {
      lat: 53.009865,
      lng: -3.2524038,
    };

    return (
      <div>
        <form>
          <select
            value={this.state.selectedAuthority}
            onChange={this.selectAuthority}
          >
            <option>Select a local authority</option>
            {this.data.features.map((feature, idx) => (
              <option
                value={idx}
                key={feature.properties ? feature.properties.lad17cd : ''}
              >
                {feature.properties ? feature.properties.lad17nm : ''}
              </option>
            ))}
          </select>
        </form>
        <Map
          ref={(n: any) => (this.mapNode = n)}
          center={position}
          className={styles.map}
          zoom={6}
        >
          <GeoJSON
            data={this.data}
            onEachFeature={this.onEachFeature}
            style={this.styleFeature}
            onClick={this.click}
          />
        </Map>
      </div>
    );
  }
}

export default PrototypeMap;
