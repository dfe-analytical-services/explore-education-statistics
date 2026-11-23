import useMappingAccordionControls from '@admin/pages/release/data/hooks/useMappingAccordionControls';
import { act, renderHook } from '@testing-library/react';

test('preserves defaults and applies the last bulk action to dynamically added sections', () => {
  const { result, rerender } = renderHook(
    ({ sections }) => useMappingAccordionControls(sections),
    {
      initialProps: {
        sections: [
          { id: 'new', open: true },
          { id: 'auto', open: false },
        ],
      },
    },
  );
  expect(
    result.current.getAccordionProps(['new', 'auto']).openSectionIds,
  ).toEqual(['new']);
  act(() => result.current.toggleAll());
  expect(result.current.allExpanded).toBe(true);
  act(() => result.current.toggleAll());
  rerender({
    sections: [
      { id: 'new', open: true },
      { id: 'added', open: true },
    ],
  });
  expect(
    result.current.getAccordionProps(['new', 'added']).openSectionIds,
  ).toEqual([]);
  act(() =>
    result.current.getAccordionProps(['new']).onOpenSectionIdsChange?.(['new']),
  );
  expect(result.current.allExpanded).toBe(false);
  act(() => result.current.toggleAll());
  rerender({
    sections: [
      { id: 'added', open: true },
      { id: 'another', open: false },
    ],
  });
  expect(
    result.current.getAccordionProps(['added', 'another']).openSectionIds,
  ).toEqual(['added', 'another']);
});
