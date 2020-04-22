import { LocationLevelKeys } from '@common/services/tableBuilderService';
import { PartialRecord } from '@common/types/util';

export interface Location {
  code: string;
  name: string;
}

export interface LocalAuthority extends Location {
  old_code: string;
}

export interface DataBlockLocation
  extends PartialRecord<LocationLevelKeys, Location> {
  localAuthority?: LocalAuthority;

  // I don't like using any, but it's required here to simplify mapping to the Table Tool, for now
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  [key: string]: any;
}

export interface DataBlockRerequest {
  boundaryLevel?: number;
}
