import isEqual from 'lodash/isEqual';
import { produce } from 'immer';
import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';
import { MapItem } from '../PrototypePrepareNextSubjectPage';

export interface Items {
  mappedItems: [MapItem, MapItem][];
  newItems: MapItem[];
  noMappingItems: MapItem[];
  unmappedItems: MapItem[];
}

export type VersionType = 'major' | 'minor';
export type FacetType = 'location' | 'filter' | 'indicator';

export interface Changelog {
  locations: Items;
  filters: Items;
  indicators: Items;
  versionNotes?: string;
  versionType: VersionType;
}

export interface PrototypeNextSubjectContextState {
  locations: Items;
  filters: Items;
  indicators: Items;
  versionNotes?: string;
  versionType: VersionType;
  onMapItem: (id: string, itemToMap: MapItem, type: FacetType) => void;
  onUnmapItem: (itemToUnMap: [MapItem, MapItem], type: FacetType) => void;
  onRemoveNoMappingItem: (item: MapItem, type: FacetType) => void;
  onAddNoMappingItem: (item: MapItem, type: FacetType) => void;
  setVersionNotes: (notes?: string) => void;
  setVersionType: (type: VersionType) => void;
}

const PrototypeNextSubjectContext = createContext<
  PrototypeNextSubjectContextState | undefined
>(undefined);

interface PrototypeNextSubjectContextProviderProps {
  children:
    | ReactNode
    | ((value: PrototypeNextSubjectContextState) => ReactNode);
  locations: Items;
  filters: Items;
  indicators: Items;
  versionType: VersionType;
}

export const PrototypeNextSubjectContextProvider = ({
  children,
  locations: initialLocations,
  filters: initialFilters,
  indicators: initialIndicators,
  versionType: initialVersionType,
}: PrototypeNextSubjectContextProviderProps) => {
  const [locations, setLocations] = useState<Items>(initialLocations);
  const [filters, setFilters] = useState<Items>(initialFilters);
  const [indicators, setIndicators] = useState<Items>(initialIndicators);
  const [versionType, setVersionType] =
    useState<VersionType>(initialVersionType);
  const [versionNotes, setVersionNotes] = useState<string | undefined>('');

  const value = useMemo<PrototypeNextSubjectContextState>(() => {
    const onMapItem: PrototypeNextSubjectContextState['onMapItem'] = (
      id,
      itemToMap,
      type,
    ) => {
      if (type === 'location') {
        const newItem = locations.newItems.find(item => item.id === id);
        if (!newItem) {
          return;
        }
        const updated = produce(locations, draft => {
          draft.newItems = locations.newItems.filter(item => item.id !== id);
          draft.mappedItems = [[itemToMap, newItem], ...locations.mappedItems];
          draft.unmappedItems = locations.unmappedItems.filter(
            item => item.id !== itemToMap.id,
          );
        });
        setLocations(updated);
      }

      if (type === 'filter') {
        const newItem = filters.newItems.find(item => item.id === id);
        if (!newItem) {
          return;
        }
        const updated = produce(filters, draft => {
          draft.newItems = filters.newItems.filter(item => item.id !== id);
          draft.mappedItems = [[itemToMap, newItem], ...filters.mappedItems];
          draft.unmappedItems = filters.unmappedItems.filter(
            item => item.id !== itemToMap.id,
          );
        });
        setFilters(updated);
      }

      if (type === 'indicator') {
        const newItem = indicators.newItems.find(item => item.id === id);
        if (!newItem) {
          return;
        }
        const updated = produce(indicators, draft => {
          draft.newItems = indicators.newItems.filter(item => item.id !== id);
          draft.mappedItems = [[itemToMap, newItem], ...indicators.mappedItems];
          draft.unmappedItems = indicators.unmappedItems.filter(
            item => item.id !== itemToMap.id,
          );
        });
        setIndicators(updated);
      }
    };

    const onUnmapItem: PrototypeNextSubjectContextState['onUnmapItem'] = (
      itemToUnmap,
      type,
    ) => {
      if (type === 'location') {
        const updated = produce(locations, draft => {
          draft.newItems = [itemToUnmap[1], ...locations.newItems];
          draft.mappedItems = locations.mappedItems.filter(
            item => !isEqual(item, itemToUnmap),
          );
          draft.unmappedItems = [itemToUnmap[0], ...locations.unmappedItems];
        });
        setLocations(updated);
      }

      if (type === 'filter') {
        const updated = produce(filters, draft => {
          draft.newItems = [itemToUnmap[1], ...filters.newItems];
          draft.mappedItems = filters.mappedItems.filter(
            item => !isEqual(item, itemToUnmap),
          );
          draft.unmappedItems = [itemToUnmap[0], ...filters.unmappedItems];
        });
        setFilters(updated);
      }

      if (type === 'indicator') {
        const updated = produce(indicators, draft => {
          draft.newItems = [itemToUnmap[1], ...indicators.newItems];
          draft.mappedItems = indicators.mappedItems.filter(
            item => !isEqual(item, itemToUnmap),
          );
          draft.unmappedItems = [itemToUnmap[0], ...indicators.unmappedItems];
        });
        setIndicators(updated);
      }
    };

    const onRemoveNoMappingItem: PrototypeNextSubjectContextState['onRemoveNoMappingItem'] =
      (item, type) => {
        if (type === 'location') {
          const updated = produce(locations, draft => {
            draft.noMappingItems = locations.noMappingItems.filter(
              noMapppingItem => !isEqual(noMapppingItem, item),
            );
            draft.unmappedItems = [item, ...locations.unmappedItems];
          });
          setLocations(updated);
        }

        if (type === 'filter') {
          const updated = produce(filters, draft => {
            draft.noMappingItems = filters.noMappingItems.filter(
              noMapppingItem => !isEqual(noMapppingItem, item),
            );
            draft.unmappedItems = [item, ...filters.unmappedItems];
          });
          setFilters(updated);
        }

        if (type === 'indicator') {
          const updated = produce(indicators, draft => {
            draft.noMappingItems = indicators.noMappingItems.filter(
              noMapppingItem => !isEqual(noMapppingItem, item),
            );
            draft.unmappedItems = [item, ...indicators.unmappedItems];
          });
          setIndicators(updated);
        }
      };

    const onAddNoMappingItem: PrototypeNextSubjectContextState['onAddNoMappingItem'] =
      (item, type) => {
        if (type === 'location') {
          const updated = produce(locations, draft => {
            draft.noMappingItems = [item, ...locations.noMappingItems];
            draft.unmappedItems = locations.unmappedItems.filter(
              unmappedItem => unmappedItem.id !== item.id,
            );
          });
          setLocations(updated);
        }

        if (type === 'filter') {
          const updated = produce(filters, draft => {
            draft.noMappingItems = [item, ...filters.noMappingItems];
            draft.unmappedItems = filters.unmappedItems.filter(
              unmappedItem => unmappedItem.id !== item.id,
            );
          });
          setFilters(updated);
        }

        if (type === 'indicator') {
          const updated = produce(indicators, draft => {
            draft.noMappingItems = [item, ...indicators.noMappingItems];
            draft.unmappedItems = indicators.unmappedItems.filter(
              unmappedItem => unmappedItem.id !== item.id,
            );
          });
          setIndicators(updated);
        }
      };

    return {
      locations,
      filters,
      indicators,
      versionNotes,
      versionType,
      onMapItem,
      onUnmapItem,
      onRemoveNoMappingItem,
      onAddNoMappingItem,
      setVersionNotes,
      setVersionType,
    };
  }, [
    locations,
    filters,
    indicators,
    versionNotes,
    versionType,
    setVersionType,
    setVersionNotes,
  ]);

  return (
    <PrototypeNextSubjectContext value={value}>
      {typeof children === 'function' ? children(value) : children}
    </PrototypeNextSubjectContext>
  );
};

export function usePrototypeNextSubjectContext() {
  const context = useContext(PrototypeNextSubjectContext);

  if (!context) {
    throw new Error(
      'usePrototypeNextSubjectContext must be used within a PrototypeNextSubjectContextProvider',
    );
  }
  return context;
}
