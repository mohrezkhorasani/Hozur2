// src/components/EmojiIcon.jsx
import React, { useState } from 'react';
import { combinePath } from '../Tools/tools';
import { BaseURLFiles } from '../api/apiNeed';

/**
 * Displays a single emoji PNG from the Emoji-pack assets or a custom uploaded room avatar.
 * @param {string} name - the png filename, brand/emoji path, or custom upload path/URL
 * @param {string} className - optional extra CSS classes
 * @param {object} style - optional inline styles
 */
const EmojiIcon = ({ name, className = "", style = {} }) => {
  const [hasError, setHasError] = useState(false);

  if (!name || hasError) {
    return null;
  }

  const cleanName = String(name).trim();
  let src = '';

  if (
    cleanName.startsWith('http://') ||
    cleanName.startsWith('https://') ||
    cleanName.startsWith('data:') ||
    cleanName.startsWith('blob:')
  ) {
    src = cleanName;
  } else if (cleanName.startsWith('/uploads/') || cleanName.startsWith('/img/')) {
    src = combinePath(BaseURLFiles, cleanName);
  } else {
    // Check if filename already includes an image extension
    const hasExtension = /\.(png|jpe?g|webp|svg|gif|ico)$/i.test(cleanName);
    const fileName = hasExtension ? cleanName : `${cleanName}.png`;
    const basePath = `/img/rooms/icons/${fileName.replace(/^\//, '')}`;
    src = combinePath(BaseURLFiles, basePath);
  }

  return (
    <img
      src={src}
      alt={cleanName}
      onError={() => setHasError(true)}
      className={`emoji-icon inline-block object-contain rounded-md ${className}`}
      style={{ width: "1.5em", height: "1.5em", verticalAlign: "middle", ...style }}
      loading="lazy"
    />
  );
};

export default EmojiIcon;
