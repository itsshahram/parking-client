

namespace Parking.App.Helpers;

//public unsafe class FFmpegHelper
//{
//    public FFmpegHelper()
//    {
//        // مقداردهی اولیه FFmpeg
//        ffmpeg.avformat_network_init();
//    }

//    public Bitmap GetFrameFromRTSP(string rtspUrl)
//    {
//        AVFormatContext* formatContext = ffmpeg.avformat_alloc_context();

//        // تنظیم گزینه‌های RTSP
//        AVDictionary* options = null;
//        ffmpeg.av_dict_set(&options, "rtsp_transport", "tcp", 0); // استفاده از TCP
//        ffmpeg.av_dict_set(&options, "stimeout", "5000000", 0);  // تنظیم Timeout به 5 ثانیه

//        if (ffmpeg.avformat_open_input(&formatContext, rtspUrl, null, &options) != 0)
//        {
//            throw new Exception("Cannot open RTSP stream.");
//        }

//        if (ffmpeg.avformat_find_stream_info(formatContext, null) < 0)
//        {
//            throw new Exception("Cannot find stream info.");
//        }


//        // یافتن استریم ویدئو
//        int videoStreamIndex = -1;
//        for (int i = 0; i < formatContext->nb_streams; i++)
//        {
//            if (formatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
//            {
//                videoStreamIndex = i;
//                break;
//            }
//        }

//        if (videoStreamIndex == -1)
//        {
//            throw new Exception("Cannot find video stream.");
//        }

//        // تنظیم کدک پارامترها
//        AVCodecParameters* codecParameters = formatContext->streams[videoStreamIndex]->codecpar;
//        AVCodec* codec = ffmpeg.avcodec_find_decoder(codecParameters->codec_id);

//        if (codec == null)
//        {
//            throw new Exception("Codec not found.");
//        }

//        AVCodecContext* codecContext = ffmpeg.avcodec_alloc_context3(codec);
//        if (ffmpeg.avcodec_parameters_to_context(codecContext, codecParameters) < 0)
//        {
//            throw new Exception("Failed to copy codec parameters to codec context.");
//        }

//        if (ffmpeg.avcodec_open2(codecContext, codec, null) < 0)
//        {
//            throw new Exception("Cannot open codec.");
//        }

//        // آماده‌سازی فریم‌ها
//        AVPacket* packet = ffmpeg.av_packet_alloc();
//        AVFrame* frame = ffmpeg.av_frame_alloc();
//        AVFrame* rgbFrame = ffmpeg.av_frame_alloc();

//        // تنظیم بافر برای تبدیل فریم به RGB
//        int bufferSize = ffmpeg.av_image_get_buffer_size(
//            AVPixelFormat.AV_PIX_FMT_RGB24,
//            codecContext->width,
//            codecContext->height,
//            1);

//        byte[] buffer = new byte[bufferSize];
//        fixed (byte* pBuffer = buffer)
//        {
//            // ایجاد آرایه‌هایی برای ارسال به متد
//            byte_ptrArray4 dataArray = new byte_ptrArray4();
//            int_array4 linesizeArray = new int_array4();

//            // مقداردهی اولیه آرایه‌ها
//            for (uint i = 0; i < 4; i++)
//            {
//                dataArray[i] = rgbFrame->data[i];
//                linesizeArray[i] = rgbFrame->linesize[i];
//            }

//            ffmpeg.av_image_fill_arrays(
//                ref dataArray,
//                ref linesizeArray,
//                pBuffer,
//                AVPixelFormat.AV_PIX_FMT_RGB24,
//                codecContext->width,
//                codecContext->height,
//                1);

//            // انتقال داده‌های برگشتی به rgbFrame
//            for (uint i = 0; i < 4; i++)
//            {
//                rgbFrame->data[i] = dataArray[i];
//                rgbFrame->linesize[i] = linesizeArray[i];
//            }
//        }




//        SwsContext* swsContext = ffmpeg.sws_getContext(
//            codecContext->width, codecContext->height, codecContext->pix_fmt,
//            codecContext->width, codecContext->height, AVPixelFormat.AV_PIX_FMT_RGB24,
//            ffmpeg.SWS_BILINEAR, null, null, null);

//        // دریافت یک فریم از جریان
//        while (ffmpeg.av_read_frame(formatContext, packet) >= 0)
//        {
//            if (packet->stream_index == videoStreamIndex)
//            {
//                if (ffmpeg.avcodec_send_packet(codecContext, packet) >= 0 &&
//                    ffmpeg.avcodec_receive_frame(codecContext, frame) >= 0)
//                {
//                    // تبدیل فریم به RGB
//                    ffmpeg.sws_scale(
//                        swsContext,
//                        frame->data,
//                        frame->linesize,
//                        0,
//                        codecContext->height,
//                        rgbFrame->data,
//                        rgbFrame->linesize);

//                    // ساختن Bitmap از فریم RGB
//                    var bitmap = new Bitmap(
//                        codecContext->width,
//                        codecContext->height,
//                        rgbFrame->linesize[0],
//                        System.Drawing.Imaging.PixelFormat.Format24bppRgb,
//                        (IntPtr)rgbFrame->data[0]);

//                    // آزاد کردن منابع بسته
//                    ffmpeg.av_packet_unref(packet);

//                    return bitmap;
//                }
//            }
//            ffmpeg.av_packet_unref(packet);
//        }

//        // آزاد کردن منابع
//        ffmpeg.sws_freeContext(swsContext);
//        ffmpeg.av_frame_free(&frame);
//        ffmpeg.av_frame_free(&rgbFrame);
//        ffmpeg.av_packet_free(&packet);
//        ffmpeg.avcodec_close(codecContext);
//        ffmpeg.avformat_close_input(&formatContext);

//        throw new Exception("No frame received.");
//    }
//}
