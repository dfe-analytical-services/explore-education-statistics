interface Props {
  className?: string;
}
export default function SubscribeIcon({ className }: Props) {
  return (
    <svg
      className={className}
      aria-hidden="true"
      focusable="false"
      xmlns="http://www.w3.org/2000/svg"
      height="18"
      width="18"
      viewBox="0 0 459.334 459.334"
    >
      <path
        fill="currentColor"
        d="M177.216 404.514c-.001.12-.009.239-.009.359 0 30.078 24.383 54.461 54.461 54.461s54.461-24.383 54.461-54.461c0-.12-.008-.239-.009-.359H175.216zM403.549 336.438l-49.015-72.002v-89.83c0-60.581-43.144-111.079-100.381-122.459V24.485C254.152 10.963 243.19 0 229.667 0s-24.485 10.963-24.485 24.485v27.663c-57.237 11.381-100.381 61.879-100.381 122.459v89.83l-49.015 72.002a24.76 24.76 0 0 0 20.468 38.693H383.08a24.761 24.761 0 0 0 20.469-38.694z"
      />
    </svg>
  );
}
