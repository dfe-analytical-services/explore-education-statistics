
function generateUIThemeName(): string {
    const date = new Date();
    const utcFormat = date.toISOString().replace("T", " ").substring(0, 19);
    const uithemename = 'UI Test'.concat(utcFormat);
    return uithemename;
}

// Export the function
export default generateUIThemeName;
