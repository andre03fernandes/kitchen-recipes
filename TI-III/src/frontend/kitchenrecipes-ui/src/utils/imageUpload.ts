const MAX_IMAGE_FILE_SIZE_BYTES = 2 * 1024 * 1024

export async function readImageFileAsDataUrl(file: File): Promise<string> {
  if (!file.type.startsWith('image/')) {
    throw new Error('invalid-type')
  }

  if (file.size > MAX_IMAGE_FILE_SIZE_BYTES) {
    throw new Error('too-large')
  }

  return await new Promise<string>((resolve, reject) => {
    const reader = new FileReader()

    reader.onload = () => {
      if (typeof reader.result === 'string') {
        resolve(reader.result)
      } else {
        reject(new Error('invalid-content'))
      }
    }

    reader.onerror = () => {
      reject(new Error('read-failed'))
    }

    reader.readAsDataURL(file)
  })
}
