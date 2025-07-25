import {
  ChartBuilderFormContextProviderProps,
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
  useChartBuilderFormsContext,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import { OmitStrict } from '@common/types';
import { renderHook, waitFor } from '@testing-library/react';
import React, { ReactNode } from 'react';

describe('useChartBuilderFormsContext', () => {
  interface Props
    extends OmitStrict<ChartBuilderFormContextProviderProps, 'children'> {
    children?: ReactNode;
  }

  test('has correct initial state', () => {
    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  test('has correct initial state if `definition` has legend capability', () => {
    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper({
        definition: {
          axes: {},
          name: '',
          type: 'line',
          legend: {},
          options: {},
          capabilities: {
            ...lineChartBlockDefinition.capabilities,
          },
        },
      }),
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      legend: {
        title: 'Legend',
        id: 'chartBuilder-legend',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  // EES-4936 Skipped tests are because initialProps are not passed to the wrapper component,
  // so we can't test updating them using rerender.
  // https://testing-library.com/docs/react-testing-library/api/#renderhook-options-initialprops
  test.skip('has correct state if updated `definition` has legend capability', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });

    rerender({
      definition: {
        axes: {},
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      legend: {
        title: 'Legend',
        id: 'chartBuilder-legend',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  test('has correct state when merging with existing legend state', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper({
          initialForms: {
            options: {
              title: 'Chart configuration',
              id: 'chartBuilder-options',
              isValid: false,
              submitCount: 0,
            },
            legend: {
              title: 'Legend',
              id: 'chartBuilder-legend',
              isValid: true,
              submitCount: 3,
            },
          },
        }),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      legend: {
        title: 'Legend',
        id: 'chartBuilder-legend',
        isValid: true,
        submitCount: 3,
      },
    });

    rerender({
      definition: {
        axes: {},
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      legend: {
        title: 'Legend',
        id: 'chartBuilder-legend',
        isValid: true,
        submitCount: 3,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('removes correct state if updated `definition` does not have capability', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper({
          definition: {
            axes: {},
            name: '',
            type: 'line',
            legend: {},
            options: {},
            capabilities: {
              ...lineChartBlockDefinition.capabilities,
            },
          },
        }),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      legend: {
        title: 'Legend',
        id: 'chartBuilder-legend',
        isValid: false,
        submitCount: 0,
      },
    });

    rerender({
      definition: {
        axes: {},
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  test('has correct initial state if `definition` has major axis', () => {
    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper({
        definition: {
          axes: {
            major: {
              type: 'major',
              id: 'major',
              title: 'Major axis',
              defaults: {},
            },
          },
          name: '',
          type: 'line',
          legend: {},
          options: {},
          capabilities: {
            ...lineChartBlockDefinition.capabilities,
            hasLegend: false,
          },
        },
      }),
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      dataSets: {
        title: 'Data sets',
        id: 'chartBuilder-dataSets',
        isValid: false,
        submitCount: 0,
      },
      major: {
        title: 'Major axis',
        id: 'chartBuilder-major',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('has correct state if updated `definition` has major axis', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });

    rerender({
      definition: {
        axes: {
          major: {
            type: 'major',
            id: 'major',
            title: 'Major axis',
            defaults: {},
          },
        },
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      dataSets: {
        title: 'Data sets',
        id: 'chartBuilder-dataSets',
        isValid: false,
        submitCount: 0,
      },
      major: {
        title: 'Major axis',
        id: 'chartBuilder-major',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('has correct state when merging with existing major axis state', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper({
          initialForms: {
            options: {
              title: 'Chart configuration',
              id: 'chartBuilder-options',
              isValid: false,
              submitCount: 0,
            },
            dataSets: {
              title: 'Data sets',
              id: 'chartBuilder-dataSets',
              isValid: false,
              submitCount: 0,
            },
            major: {
              title: 'Major axis',
              id: 'chartBuilder-major',
              isValid: true,
              submitCount: 3,
            },
          },
        }),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      dataSets: {
        title: 'Data sets',
        id: 'chartBuilder-dataSets',
        isValid: false,
        submitCount: 0,
      },
      major: {
        title: 'Major axis',
        id: 'chartBuilder-major',
        isValid: true,
        submitCount: 3,
      },
    });

    rerender({
      definition: {
        axes: {
          major: {
            type: 'major',
            id: 'major',
            title: 'Major axis',
            defaults: {},
          },
        },
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      dataSets: {
        title: 'Data sets',
        id: 'chartBuilder-dataSets',
        isValid: false,
        submitCount: 0,
      },
      major: {
        title: 'Major axis',
        id: 'chartBuilder-major',
        isValid: true,
        submitCount: 3,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('removes correct state if updated `definition` does not have major axis', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper({
          definition: {
            axes: {
              major: {
                type: 'major',
                id: 'major',
                title: 'Major axis',
                defaults: {},
              },
            },
            name: '',
            type: 'line',
            legend: {},
            options: {},
            capabilities: {
              ...lineChartBlockDefinition.capabilities,
              hasLegend: false,
            },
          },
        }),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      dataSets: {
        title: 'Data sets',
        id: 'chartBuilder-dataSets',
        isValid: false,
        submitCount: 0,
      },
      major: {
        title: 'Major axis',
        id: 'chartBuilder-major',
        isValid: false,
        submitCount: 0,
      },
    });

    rerender({
      definition: {
        axes: {},
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  test('has correct initial state if `definition` has minor axis', () => {
    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper({
        definition: {
          axes: {
            minor: {
              type: 'minor',
              id: 'minor',
              title: 'Minor axis',
              defaults: {},
            },
          },
          name: '',
          type: 'line',
          legend: {},
          options: {},
          capabilities: {
            ...lineChartBlockDefinition.capabilities,
            hasLegend: false,
          },
        },
      }),
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      minor: {
        title: 'Minor axis',
        id: 'chartBuilder-minor',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('has correct state if new `definition` has minor axis', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });

    rerender({
      definition: {
        axes: {
          minor: {
            type: 'minor',
            id: 'minor',
            title: 'Minor axis',
            defaults: {},
          },
        },
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      minor: {
        title: 'Minor axis',
        id: 'chartBuilder-minor',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('has correct state when merging with existing minor axis state', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper({
          initialForms: {
            options: {
              title: 'Chart configuration',
              id: 'chartBuilder-options',
              isValid: false,
              submitCount: 0,
            },
            minor: {
              title: 'Minor axis',
              id: 'chartBuilder-minor',
              isValid: true,
              submitCount: 3,
            },
          },
        }),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      minor: {
        title: 'Minor axis',
        id: 'chartBuilder-minor',
        isValid: true,
        submitCount: 3,
      },
    });

    rerender({
      definition: {
        axes: {
          minor: {
            type: 'minor',
            id: 'minor',
            title: 'Minor axis',
            defaults: {},
          },
        },
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      minor: {
        title: 'Minor axis',
        id: 'chartBuilder-minor',
        isValid: true,
        submitCount: 3,
      },
    });
  });

  // EES-4936 This test doesn't work with the new version of renderHook.
  test.skip('removes correct state if updated `definition` does not have minor axis', () => {
    const { result, rerender } = renderHook(
      () => useChartBuilderFormsContext(),
      {
        wrapper: createWrapper({
          definition: {
            axes: {
              minor: {
                type: 'minor',
                id: 'minor',
                title: 'Minor axis',
                defaults: {},
              },
            },
            name: '',
            type: 'line',
            legend: {},
            options: {},
            capabilities: {
              ...lineChartBlockDefinition.capabilities,
              hasLegend: false,
            },
          },
        }),
      },
    );

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
      minor: {
        title: 'Minor axis',
        id: 'chartBuilder-minor',
        isValid: false,
        submitCount: 0,
      },
    });

    rerender({
      definition: {
        axes: {},
        name: '',
        type: 'line',
        legend: {},
        options: {},
        capabilities: {
          ...lineChartBlockDefinition.capabilities,
          hasLegend: false,
        },
      },
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.forms).toEqual<ChartBuilderForms>({
      options: {
        title: 'Chart configuration',
        id: 'chartBuilder-options',
        isValid: false,
        submitCount: 0,
      },
    });
  });

  test('updating form merges new state into the correct form', async () => {
    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper({
        definition: {
          axes: {},
          name: '',
          type: 'line',
          legend: {},
          options: {},
          capabilities: {
            ...lineChartBlockDefinition.capabilities,
          },
        },
      }),
    });

    result.current.updateForm({
      formKey: 'legend',
      isValid: true,
      submitCount: 1,
    });
    await waitFor(() => {
      expect(result.current.forms).toEqual<ChartBuilderForms>({
        options: {
          title: 'Chart configuration',
          id: 'chartBuilder-options',
          isValid: false,
          submitCount: 0,
        },
        legend: {
          title: 'Legend',
          id: 'chartBuilder-legend',
          isValid: true,
          submitCount: 1,
        },
      });
    });
  });

  test('submitting does not run `onSubmit` callback if any form is invalid', async () => {
    const handleSubmit = jest.fn();

    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper({
        onSubmit: handleSubmit,
      }),
    });

    expect(result.current.isValid).toBe(false);
    expect(result.current.isSubmitting).toBe(false);

    result.current.submitForms();

    expect(result.current.isValid).toBe(false);
    expect(result.current.isSubmitting).toBe(false);
  });

  test('submitting sets `isSubmitting` to false once callback has run', async () => {
    const { result } = renderHook(() => useChartBuilderFormsContext(), {
      wrapper: createWrapper({
        initialForms: {
          options: {
            title: 'Chart configuration',
            id: 'chartBuilder-options',
            isValid: true,
            submitCount: 0,
          },
        },
        onSubmit: jest.fn(),
      }),
    });

    expect(result.current.isSubmitting).toBe(false);

    result.current.submitForms();

    await waitFor(() => {
      expect(result.current.isSubmitting).toBe(true);
    });

    await waitFor(() => {
      expect(result.current.isSubmitting).toBe(false);
    });
  });

  function createWrapper(props?: Props) {
    return function CreatedWrapper({ children }: { children: ReactNode }) {
      return (
        <ChartBuilderFormsContextProvider {...props}>
          {children}
        </ChartBuilderFormsContextProvider>
      );
    };
  }
});
