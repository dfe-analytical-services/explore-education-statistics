import { Marker } from '@admin/types/ckeditor';

// Get the order of the markers based on their position in the editor.
// Used to order the comments list.
const getMarkersOrder = (markers: Marker[]): string[] => {
  return [...markers]
    .sort((a, b) => {
      if (a.getStart().isAfter(b.getStart())) {
        return 1;
      }
      if (a.getStart().isBefore(b.getStart())) {
        return -1;
      }
      return 0;
    })
    .map(marker =>
      marker.name.startsWith('comment:')
        ? marker.name.replace('comment:', '')
        : marker.name.replace('resolvedcomment:', ''),
    );
};
export default getMarkersOrder;
