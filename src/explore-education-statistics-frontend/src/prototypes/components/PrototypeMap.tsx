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

import { GeoJSON, Map } from 'react-leaflet';

export interface PrototypeMapProps {
  OnFeatureSelect?: any;
  map: (map: PrototypeMap) => void;
}

interface PrototypeMapState {
  selectedAuthority: string;
}

class PrototypeMap extends Component<PrototypeMapProps, PrototypeMapState> {
  private mapNode: any;

  public static defaultProps: Partial<PrototypeMapProps> = {
    OnFeatureSelect: undefined,
  };

  constructor(props: PrototypeMapProps) {
    super(props);

    if (props.map) props.map(this);

    this.state = {
      selectedAuthority: '',
    };
  }

  public refresh() {
    this.mapNode.leafletElement.invalidateSize();
  }

  public render() {
    const { OnFeatureSelect } = this.props;

    const position = {
      lat: 53.009865,
      lng: -3.2524038,
    };

    // force a refresh to fix a bug
    requestAnimationFrame(() => this.refresh());

    // @ts-ignore
    const onEachFeature = (f, layer) => {
      // @ts-ignore
      layer.bindTooltip(f.properties.lad17nm, {
        // className: f.properties.toolTipClass,
        direction: 'center',
        opacity: 1.0,
      });
    };

    /**
     * lad17cd - code
     * lad17nm - name
     */
    const data = {
      ...Boundaries,
      features: Boundaries.features.filter(g => {
        return g.properties !== null && g.properties.lad17cd[0] === 'E';
      }),
    } as FeatureCollection;

    const minOverall = data.features.reduce(
      (min, next) =>
        next.properties && next.properties.absence.overall < min
          ? next.properties.absence.overall
          : min,
      100,
    );
    const maxOverall = data.features.reduce(
      (max, next) =>
        next.properties && next.properties.absence.overall > max
          ? next.properties.absence.overall
          : max,
      0,
    );

    const range = (maxOverall - minOverall) / 5;

    data.features = data.features.map(feature => {
      if (feature.properties) {
        const rate = Math.trunc(
          (feature.properties.absence.overall - minOverall) / range,
        );
        feature.properties.className = styles[`rate${rate}`];
      }

      return feature;
    });

    data.features.sort((a, b) => {
      const c = [
        a.properties ? a.properties.lad17nm : '',
        b.properties ? b.properties.lad17nm : '',
      ];

      return c[0] < c[1] ? -1 : c[1] > c[0] ? 1 : 0;
    });

    const selectFeature = (feature: Feature) => {
      if (OnFeatureSelect && feature.properties) {
        OnFeatureSelect(feature.properties);
      }

      const featureIndex = data.features.findIndex(f => f === feature);

      this.setState({
        selectedAuthority: `${featureIndex}`,
      });
    };

    // @ts-ignore
    const click = (e: any) => {
      if (e.sourceTarget.feature) {
        selectFeature(e.sourceTarget.feature);
      }
    };

    const selectAuthority = (e: ChangeEvent<HTMLSelectElement>) => {
      const feature = data.features[parseInt(e.currentTarget.value, 10)];
      selectFeature(feature);
    };

    const styleFeature = (f: any) => {
      return { className: f.properties && f.properties.className };
    };

    return (
      <div>
        <form>
          <select
            value={this.state.selectedAuthority}
            onChange={e => selectAuthority(e)}
          >
            <option>Select a local authority</option>
            {data.features.map((feature, idx) => (
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
            data={data}
            onEachFeature={onEachFeature}
            style={styleFeature}
            onClick={click}
          />
        </Map>
      </div>
    );
  }
}

export default PrototypeMap;
