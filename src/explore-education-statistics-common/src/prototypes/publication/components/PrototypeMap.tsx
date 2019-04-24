import { Feature } from 'geojson';
import { LatLngBounds, Path } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import React, { Component, RefAttributes } from 'react';
import { GeoJSON, Map } from 'react-leaflet';

import styles from './PrototypeMap.module.scss';
import {
  PrototypeMapBoundariesFeature,
  PrototypeMapBoundariesFeatureCollection,
} from './PrototypeMapBoundaries';

export type PrototypeMapProps = {
  boundaries: PrototypeMapBoundariesFeatureCollection;
  onFeatureSelect?: (
    properties: PrototypeMapBoundariesFeature['properties'],
  ) => void;
  selectedAuthority?: string;
} & RefAttributes<PrototypeMap>;

interface State {
  selectedAuthority: string;
  selectedFeature?: PrototypeMapBoundariesFeature;
}

class PrototypeMap extends Component<PrototypeMapProps, State> {
  public static defaultProps: Partial<PrototypeMapProps> = {
    onFeatureSelect: undefined,
    selectedAuthority: '',
  };

  // eslint-disable-next-line react/sort-comp
  private static DEFAULT_BOUNDS = new LatLngBounds(
    { lat: 48, lng: -6.5 },
    { lat: 60, lng: 2 },
  );

  public state: State = {
    // eslint-disable-next-line react/destructuring-assignment
    selectedAuthority: this.props.selectedAuthority || '',
    selectedFeature: undefined,
  };

  private mapRef: Map | null = null;

  public componentDidMount() {
    // force a refresh to fix a bug
    requestAnimationFrame(() => this.refresh());
  }

  public componentDidUpdate(prevProps: Readonly<PrototypeMapProps>): void {
    const { boundaries, selectedAuthority } = this.props;

    if (prevProps.selectedAuthority !== selectedAuthority) {
      const feature = boundaries.features.find(
        f => f.properties.lad17nm === selectedAuthority,
      );

      this.selectFeature(feature);
    }
  }

  private onEachFeature = (
    feature: PrototypeMapBoundariesFeature,
    layer: Path,
  ) => {
    const { boundaries } = this.props;

    if (feature.properties && feature.properties.selectable) {
      const featureIndex = boundaries.features.findIndex(f => f === feature);

      boundaries.features[featureIndex].properties.layer = layer;

      layer.bindTooltip(
        `${feature.properties.lad17nm}<br /> overall absence ${
          feature.properties.absence.overall
        }%`,
        {
          // className: f.properties.toolTipClass,
          direction: 'auto',
          opacity: 1.0,
          sticky: true,
        },
      );

      layer.setStyle({ weight: 1, opacity: 1.0 });
    }
  };

  private selectFeature = (feature?: PrototypeMapBoundariesFeature) => {
    const { onFeatureSelect } = this.props;
    const { selectedAuthority, selectedFeature } = this.state;

    if (selectedAuthority !== '' && selectedFeature) {
      const currentSelectedLayer = selectedFeature.properties.layer;

      if (currentSelectedLayer) {
        const element = currentSelectedLayer.getElement();

        if (element) {
          element.classList.remove(styles.selected);
        }
      }
    }

    if (feature && feature.properties.selectable) {
      if (onFeatureSelect && feature && feature.properties) {
        if (onFeatureSelect) {
          onFeatureSelect(feature.properties);
        }
      }

      if (feature) {
        const selectedLayer = feature.properties.layer;

        if (selectedLayer) {
          const element = selectedLayer.getElement();

          if (element) {
            element.classList.add(styles.selected);
          }

          // @ts-ignore
          this.mapRef.leafletElement.fitBounds(selectedLayer.getBounds(), {
            padding: [200, 200],
          });
          selectedLayer.bringToFront();
        }

        this.setState({
          selectedAuthority: feature.properties.lad17nm,
          selectedFeature: feature,
        });
      }
    } else {
      this.setState({
        selectedAuthority: '',
        selectedFeature: undefined,
      });

      if (this.mapRef) {
        this.mapRef.leafletElement.fitBounds(PrototypeMap.DEFAULT_BOUNDS, {
          padding: [200, 200],
        });
      }
    }
  };

  public refresh() {
    requestAnimationFrame(() => {
      if (this.mapRef) {
        this.mapRef.leafletElement.invalidateSize();
      }
    });
  }

  // private selectAuthority = (e: ChangeEvent<HTMLSelectElement>) => {
  //   const selectedFeatureName = e.currentTarget.value;
  //
  //   const feature = this.props.boundaries.features.find(
  //     f => f.properties.lad17nm === selectedFeatureName,
  //   );
  //
  //   this.selectFeature(feature);
  // };

  public render() {
    const { boundaries } = this.props;

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
          ref={el => {
            this.mapRef = el;
          }}
          center={position}
          className={styles.map}
          zoom={6.5}
          minZoom={6.5}
          zoomSnap={0.5}
          maxBounds={PrototypeMap.DEFAULT_BOUNDS}
        >
          <GeoJSON
            data={boundaries}
            onEachFeature={this.onEachFeature}
            style={(feature?: Feature) => ({
              className:
                feature && feature.properties && feature.properties.className,
            })}
            onClick={(e: {
              sourceTarget: { feature: PrototypeMapBoundariesFeature };
            }) => {
              if (e.sourceTarget.feature) {
                this.selectFeature(e.sourceTarget.feature);
              }
            }}
          />
        </Map>
      </div>
    );
  }
}

export default PrototypeMap;
