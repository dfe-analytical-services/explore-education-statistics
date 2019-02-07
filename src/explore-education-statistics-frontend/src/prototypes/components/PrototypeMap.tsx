import React, {
  ChangeEvent,
  Component,
  FormEvent,
  ReactEventHandler,
} from 'react';

import 'leaflet/dist/leaflet.css';

import styles from './PrototypeMap.module.scss';

import { Feature, FeatureCollection } from 'geojson';

import { LatLngBounds, Path } from 'leaflet';
import { GeoJSON, Map } from 'react-leaflet';

export interface PrototypeMapProps {
  Boundaries: FeatureCollection;
  OnFeatureSelect?: any;
  map: (map: PrototypeMap) => void;
  selectedAuthority?: string;
}

interface PrototypeMapState {
  selectedAuthority: string;
  selectedFeature?: Feature;
}

class PrototypeMap extends Component<PrototypeMapProps, PrototypeMapState> {
  private mapNode: any;

  private static DEFAULT_BOUNDS = new LatLngBounds(
    { lat: 48, lng: -6.5 },
    { lat: 60, lng: 2 },
  );

  public static defaultProps: Partial<PrototypeMapProps> = {
    OnFeatureSelect: undefined,
    selectedAuthority: '',
  };

  private data: FeatureCollection;
  private OnFeatureSelect: any | undefined;

  private geoRef: any;

  constructor(props: PrototypeMapProps) {
    super(props);

    if (props.map) props.map(this);

    this.state = {
      selectedAuthority: props.selectedAuthority || '',
      selectedFeature: undefined,
    };

    /**
     * lad17cd - code
     * lad17nm - name
     */

    this.data = {
      ...props.Boundaries,
      features: props.Boundaries.features.map(g => {
        if (g.properties) {
          g.properties.selectable = g.properties.lad17cd[0] === 'E';
        }

        return g;
      }),
    } as FeatureCollection;

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
    if (this.state.selectedAuthority !== '') {
      // @ts-ignore
      const currentSelectedLayer = this.state.selectedFeature.properties.layer;

      currentSelectedLayer.getElement().classList.remove(styles.selected);
    }

    // @ts-ignore
    if (feature && feature.properties.selectable) {
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
      }
    } else {
      this.setState({
        selectedAuthority: '',
        selectedFeature: undefined,
      });

      this.mapNode.leafletElement.fitBounds(PrototypeMap.DEFAULT_BOUNDS, {
        padding: [200, 200],
      });
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
  }

  public componentDidUpdate(
    prevProps: Readonly<PrototypeMapProps>,
    prevState: Readonly<PrototypeMapState>,
    snapshot?: any,
  ): void {
    if (prevProps.selectedAuthority !== this.props.selectedAuthority) {
      const feature = this.data.features.find(
        // @ts-ignore
        f => f.properties.lad17nm === this.props.selectedAuthority,
      );

      this.selectFeature(feature);
    }
  }

  public render() {
    const position = {
      lat: 53.009865,
      lng: -3.2524038,
    };

    return (
      <div>
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
          maxBounds={PrototypeMap.DEFAULT_BOUNDS}
        >
          <GeoJSON
            ref={(geo: any) => (this.geoRef = geo)}
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
